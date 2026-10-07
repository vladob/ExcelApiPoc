using ExcelApiPoc.AddIn.Models;
using ExcelApiPoc.AddIn.Services;

namespace PdfLayoutEngine.SimpleTests;

public sealed class PerformanceMaterialityTests
{
    private static readonly SignificanceMapping[] Mappings = {
        new() { TableKey = "22_1", RowNumber = 8, Source = "Náklady" },
        new() { TableKey = "22_1", RowNumber = 12, Source = "Náklady " }
    };

    [Fact]
    public void Sums_printed_rows_using_net_current_column_and_preserves_provenance()
    {
        var value = PerformanceMaterialityResolver.Resolve(Package(Statement(2025)), Mappings, "Náklady", 2025);
        Assert.Equal(300m, value.Value);
        Assert.Equal(new[] { 8, 12 }, value.Evidence.Select(x => x.RowNumber));
        Assert.Equal(new[] { 18, 22 }, value.Evidence.Select(x => x.RowOrdinal));
        Assert.All(value.Evidence, x => Assert.Equal(2, x.DataColumnOrdinal));
    }

    [Fact]
    public void Audited_and_previous_year_use_their_own_official_reports()
    {
        var package = Package(Statement(2025, amount: 100), Statement(2024, amount: 50));
        Assert.Equal(300m, PerformanceMaterialityResolver.Resolve(package, Mappings, "Náklady", 2025).Value);
        Assert.Equal(150m, PerformanceMaterialityResolver.Resolve(package, Mappings, "Náklady", 2024).Value);
        Assert.Null(PerformanceMaterialityResolver.Resolve(package, Mappings, "Náklady", 2023).Value);
    }

    [Fact]
    public void Missing_mapped_row_never_returns_a_partial_sum()
    {
        var statement = Statement(2025);
        statement.FinancialReports[0].Tables[0].Table.Values.RemoveAll(x => x.RowOrdinal == 22);
        var value = PerformanceMaterialityResolver.Resolve(Package(statement), Mappings, "Náklady", 2025);
        Assert.Null(value.Value);
        Assert.Contains("Missing", value.Problem);
    }

    [Fact]
    public void Duplicate_values_and_duplicate_mappings_are_not_double_counted()
    {
        var statement = Statement(2025);
        var values = statement.FinancialReports[0].Tables[0].Table.Values;
        values.Add(values[2]);
        Assert.Null(PerformanceMaterialityResolver.Resolve(Package(statement), Mappings, "Náklady", 2025).Value);
        Assert.Null(PerformanceMaterialityResolver.Resolve(Package(Statement(2025)),
            Mappings.Concat(new[] { Mappings[0] }), "Náklady", 2025).Value);
    }

    [Fact]
    public void Missing_mapping_and_missing_package_are_unavailable_not_zero()
    {
        Assert.Null(PerformanceMaterialityResolver.Resolve(Package(Statement(2025)), Mappings, "Tržby", 2025).Value);
        Assert.Null(PerformanceMaterialityResolver.Resolve(null!, Mappings, "Náklady", 2025).Value);
    }

    [Fact]
    public void Genuine_zero_and_negative_values_are_preserved()
    {
        Assert.Equal(0m, PerformanceMaterialityResolver.Resolve(Package(Statement(2025, amount: 0)), Mappings, "Náklady", 2025).Value);
        Assert.Equal(-30m, PerformanceMaterialityResolver.Resolve(Package(Statement(2025, amount: -10)), Mappings, "Náklady", 2025).Value);
    }

    [Fact]
    public void Selected_statement_is_respected_and_filings_are_not_added_together()
    {
        var package = Package(Statement(2025, id: 1), Statement(2025, id: 2, amount: 50));
        Assert.Equal(300m, PerformanceMaterialityResolver.Resolve(package, Mappings, "Náklady", 2025, 1).Value);
        Assert.Equal(150m, PerformanceMaterialityResolver.Resolve(package, Mappings, "Náklady", 2025).Value);
        Assert.Null(PerformanceMaterialityResolver.Resolve(package, Mappings, "Náklady", 2025, 99).Value);
    }

    [Fact]
    public void Unique_approved_filing_takes_precedence_over_newer_unapproved_filing()
    {
        var approved = Statement(2025, id: 1);
        approved.Statement.ApprovalDate = new DateTime(2026, 3, 1);
        Assert.Equal(300m, PerformanceMaterialityResolver.Resolve(
            Package(approved, Statement(2025, id: 2, amount: 50)), Mappings, "Náklady", 2025).Value);
    }

    [Fact]
    public void Consolidated_and_partial_period_statements_are_excluded()
    {
        var consolidated = Statement(2025);
        consolidated.Statement.IsConsolidated = true;
        Assert.Null(PerformanceMaterialityResolver.Resolve(Package(consolidated), Mappings, "Náklady", 2025).Value);
        var partial = Statement(2025);
        partial.Statement.PeriodFrom = "2025-07";
        Assert.Null(PerformanceMaterialityResolver.Resolve(Package(partial), Mappings, "Náklady", 2025).Value);
    }

    [Fact]
    public void Explicit_consolidated_selection_and_previous_year_preserve_scope()
    {
        var current = Statement(2025);
        current.Statement.IsConsolidated = true;
        var previous = Statement(2024, amount: 50);
        previous.Statement.IsConsolidated = true;
        var package = Package(current, previous);
        Assert.Equal(300m, PerformanceMaterialityResolver.Resolve(package, Mappings, "Náklady", 2025, 1).Value);
        Assert.Equal(150m, PerformanceMaterialityResolver.Resolve(package, Mappings, "Náklady", 2024,
            scope: current.Statement).Value);
    }

    [Fact]
    public void Unsupported_currency_and_ambiguous_reports_are_not_silently_used()
    {
        var statement = Statement(2025);
        statement.FinancialReports[0].Report.CurrencyCode = "USD";
        Assert.Null(PerformanceMaterialityResolver.Resolve(Package(statement), Mappings, "Náklady", 2025).Value);
        statement = Statement(2025);
        var ambiguous = new FinancialStatementEnvelope(statement.Statement,
            new[] { statement.FinancialReports[0], statement.FinancialReports[0] });
        Assert.Null(PerformanceMaterialityResolver.Resolve(Package(ambiguous), Mappings, "Náklady", 2025).Value);
    }

    [Fact]
    public void Different_templates_in_previous_year_use_the_matching_mapping()
    {
        var previous = Statement(2024, amount: 50);
        previous.FinancialReports[0].Template.Template.TemplateErpId = 21;
        var mappings = Mappings.Concat(new[] {
            new SignificanceMapping { TableKey = "21_1", RowNumber = 8, Source = "Náklady" }
        });
        Assert.Equal(50m, PerformanceMaterialityResolver.Resolve(Package(Statement(2025), previous), mappings, "Náklady", 2024).Value);
    }

    private static FinancialStatementEnvelope Statement(int year, long id = 1, decimal amount = 100)
    {
        var statement = new FinancialStatementDto { Id = id, PeriodFrom = year + "-01", PeriodTo = year + "-12" };
        var table = new FinancialReportTableDto { Id = id * 10, TableOrdinal = 0, Values = new() };
        foreach (int row in new[] { 18, 22 })
            for (int column = 0; column < 4; column++)
                table.Values.Add(new FinancialReportValueDto { RowOrdinal = row, DataColumnOrdinal = column,
                    NumericValue = column == 2 ? (row == 18 ? amount : 2 * amount) : 9999 });
        var report = new FinancialReportDto { Id = id * 100, TemplateId = 22, CurrencyCode = "EUR" };
        var template = new AuditTemplatePackageResponse { Template = new AuditTemplateDefinitionResponse {
            TemplateErpId = 22, Tables = new[] { new AuditReportTableDefinitionResponse {
                TableOrdinal = 0, NumberOfColumns = 7, NumberOfDataColumns = 4,
                Headers = new[] { new AuditReportHeaderDefinitionResponse { TextSk = "Predchádzajúce obdobie", ColumnPosition = 7, ColumnSpan = 1 } },
                Rows = new[] { new AuditReportRowDefinitionResponse { RowNumber = 8, RowOrdinal = 18 },
                    new AuditReportRowDefinitionResponse { RowNumber = 12, RowOrdinal = 22 } }
            } }
        } };
        return new FinancialStatementEnvelope(statement, new[] { new FinancialReportEnvelope(report, statement,
            null!, template, new[] { new FinancialReportTableEnvelope(table, report) }) });
    }

    private static AccountingEntityPackageEnvelope Package(params FinancialStatementEnvelope[] statements)
        => new(new AccountingEntityPackageDto(), statements, Array.Empty<AnnualReportEnvelope>(),
            new Dictionary<long, FinancialReportEnvelope>(), new Dictionary<long, FinancialReportTableEnvelope>(),
            new Dictionary<long, AuditTemplatePackageResponse>());
}
