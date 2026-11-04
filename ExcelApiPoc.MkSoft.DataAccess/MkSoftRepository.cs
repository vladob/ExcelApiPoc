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
        await using var framework = new SqlCommand("""
            SELECT AccountCode, AccountName_sk, AccountLevel, ParentAccountCode
            FROM Accounts.OfficialAccounts WHERE AccountFrameworkVersionId = @Version
            AND LEFT(AccountCode, 1) IN ('0','1','2','3','4') ORDER BY SortOrder;
            """, connection);
        framework.Parameters.Add("@Version", SqlDbType.Int).Value = result.AccountFrameworkVersionId;
        await using var definitions = await framework.ExecuteReaderAsync(ct);
        while (await definitions.ReadAsync(ct)) result.OfficialAccounts.Add(new OfficialAccount {
            AccountCode = definitions.GetString(0), AccountName = definitions.GetString(1),
            AccountLevel = Convert.ToInt32(definitions.GetValue(2)), ParentAccountCode = definitions.IsDBNull(3) ? null : definitions.GetString(3)
        });
        if (result.OfficialAccounts.Count == 0) throw new InvalidOperationException("No official account framework found.");
        result.Warnings.Add("Úplnosť importu MkSoftRaw nebola automaticky overená.");
        return result;
    }
}
