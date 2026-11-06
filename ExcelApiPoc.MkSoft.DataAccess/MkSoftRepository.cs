using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using AccountBalanceVerification.Core;
namespace ExcelApiPoc.MkSoft.DataAccess;
public sealed class MkSoftRepository
{
    private readonly string connectionString;
    public MkSoftRepository(string connectionString) { this.connectionString = connectionString; }
    public async Task<System.Collections.Generic.List<MkSoftEntity>> FindEntitiesAsync(int? clientErpId, string ico, int fiscalYear, CancellationToken ct)
    {
        // By ID may also carry an IČO constraint. No arbitrary TOP(1) for ambiguous IČO matches.
        var source = clientErpId.HasValue
            ? "[MkSoft].[EntityInfo](@ClientErpId, @FiscalYear)"
            : "[MkSoft].[EntitiesByIco](@Ico, @FiscalYear)";
        var sql = $"""
            SELECT e.ClientErpId, e.ICO, e.NAZOV1, e.PRAVNAFORMA, af.Code
            FROM {source} e
            LEFT JOIN Accounts.AccountFrameworkVersion v ON v.Id =
                CASE WHEN e.PRAVNAFORMA = 119 THEN 3 WHEN e.PRAVNAFORMA = 801 THEN 2 ELSE 1 END
            LEFT JOIN Accounts.AccountFramework af ON af.Id = v.AccountFrameworkId
            WHERE (@Ico IS NULL OR TRY_CONVERT(int, e.ICO) = @Ico)
            ORDER BY e.ClientErpId;
            """;
        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync(ct);
        await using var command = new SqlCommand(sql, connection) { CommandTimeout = 120 };
        command.Parameters.Add("@ClientErpId", SqlDbType.Int).Value = (object)clientErpId ?? DBNull.Value;
        command.Parameters.Add("@Ico", SqlDbType.Int).Value = string.IsNullOrEmpty(ico) ? DBNull.Value : (object)int.Parse(ico);
        command.Parameters.Add("@FiscalYear", SqlDbType.Int).Value = fiscalYear;
        var entities = new System.Collections.Generic.List<MkSoftEntity>();
        await using var reader = await command.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct)) entities.Add(new MkSoftEntity {
            ClientErpId = reader.GetInt32(0), FiscalYear = fiscalYear,
            Ico = reader.IsDBNull(1) ? "" : Convert.ToString(reader.GetValue(1)).Trim().PadLeft(8, '0'),
            EntityName = reader.IsDBNull(2) ? "" : Convert.ToString(reader.GetValue(2)).Trim(),
            LegalForm = reader.IsDBNull(3) ? 0 : Convert.ToInt32(reader.GetValue(3)),
            FrameworkCode = reader.IsDBNull(4) ? "" : reader.GetString(4)
        });
        return entities;
    }

    public async Task<System.Collections.Generic.List<OfficialAccount>> GetAccountingFrameworkAsync(int clientErpId, int fiscalYear, CancellationToken ct)
    {
        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync(ct);
        await using var command = new SqlCommand("SELECT UCET, NAZOV FROM [MkSoft].[AccountingFramework](@ClientErpId, @FiscalYear) ORDER BY UCET", connection) { CommandTimeout = 120 };
        command.Parameters.Add("@ClientErpId", SqlDbType.Int).Value = clientErpId;
        command.Parameters.Add("@FiscalYear", SqlDbType.Int).Value = fiscalYear;
        var result = new System.Collections.Generic.List<OfficialAccount>();
        await using var reader = await command.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct))
        {
            var code = Convert.ToString(reader.GetValue(0)).Trim();
            if (string.IsNullOrEmpty(code)) continue;
            result.Add(new OfficialAccount { AccountCode = code, AccountName = reader.IsDBNull(1) ? "" : Convert.ToString(reader.GetValue(1)).Trim(),
                AccountLevel = code.Length, ParentAccountCode = code.Length > 1 ? code.Substring(0, code.Length - 1) : null });
        }
        if (result.Count == 0) throw new InvalidOperationException("POSNOVA neobsahuje účtovnú osnovu pre zvoleného klienta a rok. Skontrolujte synchronizáciu.");
        return result;
    }

    public async Task<VerificationPackage> GetAsync(int clientErpId, int fiscalYear, bool includeEmptyAccounts, CancellationToken ct)
    {
        const string sql = """
            SELECT PRAVNAFORMA FROM [MkSoft].[EntityLegalForm](@ClientErpId, @FiscalYear);
            SELECT AccountNo, AccountName, OpeningDebit, OpeningCredit, MovementsDebit, MovementsCredit, ClosingDebit, ClosingCredit
            FROM [MkSoft].[GeneralLedger](@ClientErpId, @FiscalYear, @IncludeEmptyAccounts) ORDER BY AccountNo;
            """;
        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync(ct);
        await using var command = new SqlCommand(sql, connection) { CommandTimeout = 120 };
        command.Parameters.Add("@ClientErpId", SqlDbType.Int).Value = clientErpId;
        command.Parameters.Add("@FiscalYear", SqlDbType.Int).Value = fiscalYear;
        command.Parameters.Add("@IncludeEmptyAccounts", SqlDbType.Bit).Value = includeEmptyAccounts;
        var result = new VerificationPackage { ClientErpId = clientErpId, FiscalYear = fiscalYear, RetrievedAtUtc = DateTime.UtcNow };
        await using (var reader = await command.ExecuteReaderAsync(ct))
        {
            if (!await reader.ReadAsync(ct)) return null;
            if (reader.IsDBNull(0)) throw new InvalidOperationException("Entity legal form is missing.");
            result.LegalForm = Convert.ToInt32(reader.GetValue(0));
            if (await reader.ReadAsync(ct)) throw new InvalidOperationException("Multiple entity records returned.");
            result.AccountFrameworkVersionId = result.LegalForm == 119 ? 3 : result.LegalForm == 801 ? 2 : 1;
            await reader.NextResultAsync(ct);
            while (await reader.ReadAsync(ct)) result.Rows.Add(new LedgerRow {
                AccountNo = reader.GetString(0).Trim(), AccountName = reader.IsDBNull(1) ? "" : reader.GetString(1),
                OpeningDebit = reader.GetDecimal(2), OpeningCredit = reader.GetDecimal(3),
                MovementsDebit = reader.GetDecimal(4), MovementsCredit = reader.GetDecimal(5),
                ClosingDebit = reader.GetDecimal(6), ClosingCredit = reader.GetDecimal(7)
            });
        }
        result.OfficialAccounts = await GetAccountingFrameworkAsync(clientErpId, fiscalYear, ct);
        result.Warnings.Add("Úplnosť importu MkSoftRaw nebola automaticky overená.");
        return result;
    }
}
