using ExcelApiPoc.AddIn.Models;
using ExcelApiPoc.AddIn.Services;
using System;
using Xunit;

namespace ExcelApiPoc.AddIn.Tests
{
    public sealed class AuditCalculationPackageValidatorTests
    {
        [Fact]
        public void Validate_AcceptsResolvedTemplate690Package()
        {
            AuditCalculationPackageValidator.Validate(ValidPackage(), 2024);
        }

        [Fact]
        public void Validate_RejectsMissingResolvedIdentities()
        {
            AuditCalculationPackageResponse response = ValidPackage();
            response.CalculationPackage.CalculationConfigurationVersionId = 0;
            Assert.Throws<InvalidOperationException>(() =>
                AuditCalculationPackageValidator.Validate(response, 2024));
        }

        [Fact]
        public void Validate_RejectsTemplateMismatch()
        {
            AuditCalculationPackageResponse response = ValidPackage();
            response.RegisterUzTemplateId = 699;
            Assert.Throws<InvalidOperationException>(() =>
                AuditCalculationPackageValidator.Validate(response, 2024));
        }

        private static AuditCalculationPackageResponse ValidPackage()
        {
            return new AuditCalculationPackageResponse
            {
                ContractVersion = 1,
                FinancialStatementId = 1,
                FinancialReportId = 2,
                RegisterUzTemplateId = 690,
                CalculationPackage = new AuditTemplatePackageResponse
                {
                    ContractVersion = 5,
                    TemplateFrameworkVersionId = 3,
                    AccountFrameworkId = 4,
                    FrameworkCode = "GOV_LOCAL",
                    AccountFrameworkVersionId = 5,
                    CalculationConfigurationVersionId = 6,
                    ApplicableDate = new DateTime(2024, 12, 31),
                    Template = new AuditTemplateDefinitionResponse { TemplateErpId = 690 }
                }
            };
        }
    }
}
