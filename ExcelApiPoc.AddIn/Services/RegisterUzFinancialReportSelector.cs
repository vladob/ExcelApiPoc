using System;
using System.Collections.Generic;
using System.Globalization;
using ExcelApiPoc.AddIn.Models;

namespace ExcelApiPoc.AddIn.Services
{
    internal static class RegisterUzFinancialReportSelector
    {
        public static RegisterUzFinancialReportSelection Select(
            AccountingEntityPackageEnvelope envelope,
            int fiscalYear,
            long financialStatementId,
            long financialReportId,
            int templateErpId)
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

                if (statement.Id != financialStatementId ||
                    !string.Equals(
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

                    if (report.Id != financialReportId ||
                        report.TemplateId != templateErpId ||
                        !reportEnvelope.HasTemplate)
                    {
                        continue;
                    }

                    candidates.Add(
                        new RegisterUzFinancialReportSelection
                        {
                            Statement = statementEnvelope,
                            Report = reportEnvelope,
                            TemplateErpId = templateErpId,
                            RegisterUzReportId = report.Id
                        });
                }
            }

            if (candidates.Count == 0)
            {
                throw new InvalidOperationException(
                    $"The calculation-package API selected RegisterUZ report " +
                    $"{financialReportId} with template {templateErpId}, but " +
                    $"that report was not found in the accounting-entity " +
                    $"package for fiscal year {fiscalYear}.");
            }

            if (candidates.Count > 1)
            {
                throw new InvalidOperationException(
                    $"RegisterUZ report {financialReportId} occurred more than " +
                    "once in the accounting-entity package.");
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
