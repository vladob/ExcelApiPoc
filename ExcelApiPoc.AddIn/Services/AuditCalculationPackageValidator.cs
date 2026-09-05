using ExcelApiPoc.AddIn.Models;
using System;

namespace ExcelApiPoc.AddIn.Services
{
    public static class AuditCalculationPackageValidator
    {
        public static void Validate(AuditCalculationPackageResponse result, int fiscalYear)
        {
            if (result == null || result.ContractVersion != 1 || result.CalculationPackage == null)
                throw new InvalidOperationException("The API returned an unsupported calculation-package contract.");
            if (result.CalculationPackage.ContractVersion != 5)
                throw new InvalidOperationException("The API returned an unsupported template-package contract.");
            if (result.FinancialStatementId <= 0 || result.FinancialReportId <= 0 || result.RegisterUzTemplateId <= 0)
                throw new InvalidOperationException("The calculation package does not identify its RegisterUZ source.");
            if (result.CalculationPackage.Template == null ||
                result.CalculationPackage.Template.TemplateErpId != result.RegisterUzTemplateId)
                throw new InvalidOperationException("The selected RegisterUZ template does not match the calculation package.");
            if (result.CalculationPackage.TemplateFrameworkVersionId <= 0 ||
                result.CalculationPackage.AccountFrameworkId <= 0 ||
                result.CalculationPackage.AccountFrameworkVersionId <= 0 ||
                result.CalculationPackage.CalculationConfigurationVersionId <= 0 ||
                string.IsNullOrWhiteSpace(result.CalculationPackage.FrameworkCode))
                throw new InvalidOperationException("The calculation package does not contain complete configuration identities.");
            DateTime applicableDate = new DateTime(fiscalYear, 12, 31);
            if (!result.CalculationPackage.ApplicableDate.HasValue ||
                result.CalculationPackage.ApplicableDate.Value.Date != applicableDate)
                throw new InvalidOperationException("The calculation package is not applicable to the selected fiscal year.");
        }
    }
}
