using System;
using System.Net;
using System.Net.Http;
using ExcelApiPoc.AddIn.Models;
using Newtonsoft.Json;

namespace ExcelApiPoc.AddIn.Services
{
    internal static class AccountingEntityPackageApiClient
    {
        private static readonly HttpClient HttpClient =
            new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(30)
            };

        public static AccountingEntityPackageDto GetPackage(string ico)
        {
            if (string.IsNullOrWhiteSpace(ico))
            {
                throw new ArgumentException(
                    "IČO is required.",
                    nameof(ico));
            }

            string normalizedIco = ico.Trim();

            string relativePath =
                $"api/v1/accounting-entities/{Uri.EscapeDataString(normalizedIco)}/package";

            using (HttpResponseMessage response = HttpClient
                .GetAsync(SettingsService.BuildApiUri(relativePath))
                .GetAwaiter()
                .GetResult())
            {
                if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    throw new AccountingEntityPackageNotFoundException(
                        normalizedIco);
                }

                if (response.StatusCode == HttpStatusCode.Conflict)
                {
                    throw new AccountingEntityPackageAmbiguousException(
                        normalizedIco);
                }

                response.EnsureSuccessStatusCode();

                string json = response.Content
                    .ReadAsStringAsync()
                    .GetAwaiter()
                    .GetResult();

                AccountingEntityPackageDto package =
                    JsonConvert.DeserializeObject<AccountingEntityPackageDto>(json);

                if (package == null)
                {
                    throw new InvalidOperationException(
                        $"API returned an empty accounting-entity package for IČO {normalizedIco}.");
                }

                return package;
            }
        }

        public static AccountingEntityPackageEnvelope GetEnvelope(string ico)
        {
            AccountingEntityPackageDto package =
                GetPackage(ico);

            return AccountingEntityPackageEnvelopeBuilder.Build(package);
        }

        public static string GetEnvelopeSummary(string ico)
        {
            AccountingEntityPackageEnvelope envelope =
                GetEnvelope(ico);

            return string.Join(
                Environment.NewLine,
                $"Entity: {envelope.Entity?.Ico} - {envelope.Entity?.Name}",
                $"Financial statements: {envelope.FinancialStatements.Count}",
                $"Annual reports: {envelope.AnnualReports.Count}",
                $"Financial reports: {envelope.FinancialReportCount}",
                $"  from statements: {envelope.FinancialStatementReportCount}",
                $"  from annual reports: {envelope.AnnualReportFinancialReportCount}",
                $"Annual-report attachments: {envelope.AnnualReportAttachmentCount}",
                $"Financial-report attachments: {envelope.FinancialReportAttachmentCount}",
                $"Tables: {envelope.FinancialReportTableCount}",
                $"Templates loaded: {envelope.TemplatesById.Count}",
                $"Missing template IDs: {envelope.MissingTemplateIds.Count}",
                $"Reports with unresolved template: {envelope.UnresolvedTemplateReportCount}");
        }
    }

    internal sealed class AccountingEntityPackageNotFoundException
        : Exception
    {
        public AccountingEntityPackageNotFoundException(string ico)
            : base($"Accounting entity {ico} was not found.")
        {
            Ico = ico;
        }

        public string Ico { get; }
    }

    internal sealed class AccountingEntityPackageAmbiguousException : Exception
    {
        public AccountingEntityPackageAmbiguousException(string ico)
            : base(
                "There are multiple accounting entities with this IČO.\r\n" +
                "This functionality is not supported in this version!")
        {
            Ico = ico;
        }

        public string Ico { get; }
    }

}