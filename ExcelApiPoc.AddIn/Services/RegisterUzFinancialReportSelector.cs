using System;
using System.Collections.Generic;
using System.Globalization;
using ExcelApiPoc.AddIn.Models;

namespace ExcelApiPoc.AddIn.Services
{
    internal static class RegisterUzFinancialReportSelector
    {
        private const long SupportedTemplateId = 690;

        public static RegisterUzFinancialReportSelection Select(
            AccountingEntityPackageEnvelope envelope,
            int fiscalYear)
        {
            if (envelope == null)
            {
                throw new ArgumentNullException(nameof(envelope));
            }

            List<RegisterUzFinancialReportSelection> candidates =
                new List<RegisterUzFinancialReportSelection>();

            string expectedPeriodFrom =
                fiscalYear.ToString(CultureInfo.InvariantCulture) + "-01";

            string expectedPeriodTo =
                fiscalYear.ToString(CultureInfo.InvariantCulture) + "-12";

            foreach (FinancialStatementEnvelope statementEnvelope
                     in envelope.FinancialStatements)
            {
                FinancialStatementDto statement =
                    statementEnvelope.Statement;

                if (!string.Equals(
                        statement.PeriodFrom,
                        expectedPeriodFrom,
                        StringComparison.Ordinal) ||
                    !string.Equals(
                        statement.PeriodTo,
                        expectedPeriodTo,
                        StringComparison.Ordinal))
                {
                    continue;
                }

                foreach (FinancialReportEnvelope reportEnvelope
                         in statementEnvelope.FinancialReports)
                {
                    FinancialReportDto report =
                        reportEnvelope.Report;

                    if (report.TemplateId != SupportedTemplateId)
                    {
                        continue;
                    }

                    if (!reportEnvelope.HasTemplate)
                    {
                        continue;
                    }

                    candidates.Add(
                        new RegisterUzFinancialReportSelection
                        {
                            Statement = statementEnvelope,
                            Report = reportEnvelope,
                            TemplateErpId =
                                checked((int)SupportedTemplateId),
                            RegisterUzReportId =
                                report.Id
                        });
                }
            }

            if (candidates.Count == 0)
            {
                throw new InvalidOperationException(
                    $"No RegisterUZ financial report with template {SupportedTemplateId} " +
                    $"was found for fiscal year {fiscalYear}.");
            }

            if (candidates.Count > 1)
            {
                throw new InvalidOperationException(
                    $"Multiple RegisterUZ financial reports with template {SupportedTemplateId} " +
                    $"were found for fiscal year {fiscalYear}. " +
                    "Automatic correction-report selection is not implemented yet.");
            }

            return candidates[0];
        }
    }

    internal sealed class RegisterUzFinancialReportSelection
    {
        public FinancialStatementEnvelope Statement { get; set; }

        public FinancialReportEnvelope Report { get; set; }

        public int TemplateErpId { get; set; }

        public long RegisterUzReportId { get; set; }
    }
}
