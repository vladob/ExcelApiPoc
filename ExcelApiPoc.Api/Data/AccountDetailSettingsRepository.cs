using System.Data;
using System.Text.Json;
using ExcelApiPoc.Api.Models;
using Microsoft.Data.SqlClient;

namespace ExcelApiPoc.Api.Data;

public sealed class AccountDetailSettingsRepository
{
    private readonly string _connectionString;

    public AccountDetailSettingsRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("AuditAddIn")
            ?? throw new InvalidOperationException("Connection string 'AuditAddIn' is not configured.");
    }

    private SqlConnection Connect() => new(_connectionString);

    public async Task<int?> ResolveUserAsync(string apiKey, CancellationToken ct)
    {
        await using var connection = Connect();
        await connection.OpenAsync(ct);
        await using var command = new SqlCommand(
            "SELECT [Id] FROM [Settings].[Users] WHERE [ApiKey] = @Key AND [IsActive] = 1", connection);
        command.Parameters.Add("@Key", SqlDbType.VarChar, 64).Value = apiKey;
        var result = await command.ExecuteScalarAsync(ct);
        return result is null ? null : (int)result;
    }

    public async Task<AccountDetailTextSettings> GetTextsAsync(int userId, CancellationToken ct)
    {
        const string sql = """
            SELECT [Code], [DisplayNameSk], [SortOrder]
            FROM [Settings].[TextCategories] ORDER BY [SortOrder];
            SELECT [TextId], [CategoryCode], [TextSk], [SortOrder]
            FROM [Settings].[PredefinedTexts]
            WHERE [UserId] = @UserId AND [IsActive] = 1
            ORDER BY [CategoryCode], [SortOrder], [TextId];
            SELECT [Account], [CategoryCode], [TextId]
            FROM [Settings].[PredefinedTextMappings]
            WHERE [UserId] = @UserId ORDER BY [Account], [CategoryCode];
            """;
        await using var connection = Connect();
        await connection.OpenAsync(ct);
        await using var command = new SqlCommand(sql, connection);
        command.Parameters.Add("@UserId", SqlDbType.Int).Value = userId;
        await using var reader = await command.ExecuteReaderAsync(ct);
        var categories = new List<TextCategoryDto>();
        while (await reader.ReadAsync(ct))
            categories.Add(new(reader.GetString(0), reader.GetString(1), reader.GetByte(2)));
        await reader.NextResultAsync(ct);
        var texts = new List<PredefinedTextDto>();
        while (await reader.ReadAsync(ct))
            texts.Add(new(reader.GetInt32(0), reader.GetString(1), reader.GetString(2), reader.GetInt32(3)));
        await reader.NextResultAsync(ct);
        var defaults = new List<AccountTextDefaultDto>();
        while (await reader.ReadAsync(ct))
            defaults.Add(new(reader.GetString(0), reader.GetString(1), reader.GetInt32(2)));
        return new(categories, texts, defaults);
    }

    public async Task<AccountDetailLayoutDto?> GetLayoutAsync(int userId, CancellationToken ct)
    {
        const string sql = """
            SELECT TOP (1) q.[VersionNo], q.[DefinitionJson]
            FROM (
            SELECT 0 AS [Priority], v.[VersionNo], v.[DefinitionJson]
            FROM [Settings].[UserWorksheetLayouts] AS a
            JOIN [Settings].[WorksheetLayouts] AS l
              ON l.[Id] = a.[LayoutId] AND (l.[OwnerUserId] IS NULL OR l.[OwnerUserId] = a.[UserId])
            JOIN [Settings].[WorksheetLayoutVersions] AS v
              ON v.[LayoutId] = a.[LayoutId] AND v.[VersionNo] = a.[VersionNo]
            WHERE a.[UserId] = @UserId AND a.[LayoutCode] = 'ACCOUNT_DETAIL'
            UNION ALL
            SELECT 1 AS [Priority], v.[VersionNo], v.[DefinitionJson]
            FROM [Settings].[WorksheetLayoutDefaults] AS d
            JOIN [Settings].[WorksheetLayouts] AS l
              ON l.[Id] = d.[LayoutId] AND l.[OwnerUserId] IS NULL
            JOIN [Settings].[WorksheetLayoutVersions] AS v
              ON v.[LayoutId] = d.[LayoutId] AND v.[VersionNo] = d.[VersionNo]
            WHERE d.[LayoutCode] = 'ACCOUNT_DETAIL'
            ) AS q ORDER BY q.[Priority]
            """;
        // An explicit assignment takes precedence over the shared default.
        await using var connection = Connect();
        await connection.OpenAsync(ct);
        await using var command = new SqlCommand(sql, connection);
        command.Parameters.Add("@UserId", SqlDbType.Int).Value = userId;
        await using var reader = await command.ExecuteReaderAsync(ct);
        if (!await reader.ReadAsync(ct)) return null;
        using var document = JsonDocument.Parse(reader.GetString(1));
        return new(reader.GetInt32(0), document.RootElement.Clone());
    }

    public async Task<PredefinedTextDto?> CreateTextAsync(
        int userId, CreatePredefinedTextRequest request, CancellationToken ct)
    {
        await using var connection = Connect();
        await connection.OpenAsync(ct);
        await using var transaction = await connection.BeginTransactionAsync(IsolationLevel.Serializable, ct);
        const string sql = """
            IF EXISTS (SELECT 1 FROM [Settings].[TextCategories] WHERE [Code] = @Category)
            BEGIN
                DECLARE @Id int;
                SELECT @Id = ISNULL(MAX([TextId]), 0) + 1
                FROM [Settings].[PredefinedTexts] WITH (UPDLOCK, HOLDLOCK)
                WHERE [UserId] = @UserId AND [CategoryCode] = @Category;
                INSERT [Settings].[PredefinedTexts]
                    ([UserId], [CategoryCode], [TextId], [TextSk], [SortOrder])
                VALUES (@UserId, @Category, @Id, @Text, COALESCE(@SortOrder, @Id));
                SELECT @Id;
            END
            """;
        await using var command = new SqlCommand(sql, connection, (SqlTransaction)transaction);
        command.Parameters.Add("@UserId", SqlDbType.Int).Value = userId;
        command.Parameters.Add("@Category", SqlDbType.VarChar, 50).Value = request.CategoryCode;
        command.Parameters.Add("@Text", SqlDbType.NVarChar, 500).Value = request.TextSk;
        command.Parameters.Add("@SortOrder", SqlDbType.Int).Value = (object?)request.SortOrder ?? DBNull.Value;
        object? result = await command.ExecuteScalarAsync(ct);
        await transaction.CommitAsync(ct);
        if (result is null) return null;
        int id = (int)result;
        return new(id, request.CategoryCode, request.TextSk, request.SortOrder ?? id);
    }

    public async Task<bool> UpdateTextAsync(
        int userId, string category, int textId, UpdatePredefinedTextRequest request, CancellationToken ct)
    {
        const string sql = """
            UPDATE [Settings].[PredefinedTexts]
            SET [TextSk] = @Text, [SortOrder] = @SortOrder
            WHERE [UserId] = @UserId AND [CategoryCode] = @Category AND [TextId] = @TextId;
            """;
        await using var connection = Connect();
        await connection.OpenAsync(ct);
        await using var command = new SqlCommand(sql, connection);
        AddTextKey(command, userId, category, textId);
        command.Parameters.Add("@Text", SqlDbType.NVarChar, 500).Value = request.TextSk;
        command.Parameters.Add("@SortOrder", SqlDbType.Int).Value = request.SortOrder;
        return await command.ExecuteNonQueryAsync(ct) > 0;
    }

    public async Task<bool> DeleteTextAsync(int userId, string category, int textId, CancellationToken ct)
    {
        await using var connection = Connect();
        await connection.OpenAsync(ct);
        await using var command = new SqlCommand(
            "DELETE [Settings].[PredefinedTexts] WHERE [UserId] = @UserId AND [CategoryCode] = @Category AND [TextId] = @TextId", connection);
        AddTextKey(command, userId, category, textId);
        return await command.ExecuteNonQueryAsync(ct) > 0;
    }

    public async Task<bool> SetDefaultAsync(
        int userId, string account, string category, int textId, CancellationToken ct)
    {
        const string sql = """
            IF EXISTS (SELECT 1 FROM [Settings].[PredefinedTexts]
                       WHERE [UserId] = @UserId AND [CategoryCode] = @Category
                         AND [TextId] = @TextId AND [IsActive] = 1)
            BEGIN
                UPDATE [Settings].[PredefinedTextMappings]
                SET [TextId] = @TextId
                WHERE [UserId] = @UserId AND [CategoryCode] = @Category AND [Account] = @Account;
                IF @@ROWCOUNT = 0
                    INSERT [Settings].[PredefinedTextMappings] ([UserId], [CategoryCode], [Account], [TextId])
                    VALUES (@UserId, @Category, @Account, @TextId);
                SELECT CAST(1 AS bit);
            END
            """;
        await using var connection = Connect();
        await connection.OpenAsync(ct);
        await using var transaction = await connection.BeginTransactionAsync(IsolationLevel.Serializable, ct);
        await using var command = new SqlCommand(sql, connection, (SqlTransaction)transaction);
        AddTextKey(command, userId, category, textId);
        command.Parameters.Add("@Account", SqlDbType.VarChar, 3).Value = account;
        object? result = await command.ExecuteScalarAsync(ct);
        await transaction.CommitAsync(ct);
        return result is true;
    }

    public async Task<bool> DeleteDefaultAsync(int userId, string account, string category, CancellationToken ct)
    {
        await using var connection = Connect();
        await connection.OpenAsync(ct);
        await using var command = new SqlCommand(
            "DELETE [Settings].[PredefinedTextMappings] WHERE [UserId] = @UserId AND [Account] = @Account AND [CategoryCode] = @Category", connection);
        command.Parameters.Add("@UserId", SqlDbType.Int).Value = userId;
        command.Parameters.Add("@Account", SqlDbType.VarChar, 3).Value = account;
        command.Parameters.Add("@Category", SqlDbType.VarChar, 50).Value = category;
        return await command.ExecuteNonQueryAsync(ct) > 0;
    }

    private static void AddTextKey(SqlCommand command, int userId, string category, int textId)
    {
        command.Parameters.Add("@UserId", SqlDbType.Int).Value = userId;
        command.Parameters.Add("@Category", SqlDbType.VarChar, 50).Value = category;
        command.Parameters.Add("@TextId", SqlDbType.Int).Value = textId;
    }
}
