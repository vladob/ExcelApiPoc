/*
    Returns every mapped RegisterUZ financial report that can be considered for
    calculation for one IČO and full calendar fiscal year.

    The function deliberately does not select TOP (1). The API must distinguish
    zero, one and multiple candidates and must not guess.
*/

USE [AuditAddIn];
GO

CREATE OR ALTER FUNCTION [Accounts].[GetCalculationReportCandidates]
(
    @Ico varchar(20),
    @FiscalYear int
)
RETURNS TABLE
AS
RETURN
(
    SELECT
        ae.[RegisterUzEntityId],
        ae.[Ico],
        ae.[Name] AS [EntityName],
        ae.[Dic],
        ae.[OrganizationSizeCode],
        ae.[LegalFormCode],
        fs.[RegisterUzStatementId],
        fs.[PeriodFrom],
        fs.[PeriodTo],
        fs.[StatementType],
        fs.[IsConsolidated],
        fs.[IsConsolidatedCentralGovernment],
        fs.[IsSummaryPublicAdministration],
        fr.[RegisterUzFinancialReportId],
        t.[ErpId] AS [RegisterUzTemplateId],
        fr.[DataAvailability],
        ct.[TemplateId],
        ct.[AccountFrameworkId],
        af.[Code] AS [FrameworkCode],
        ct.[CalculationImplemented]
    FROM [RegisterUZ].[Registry].[AccountingEntity] AS ae
    INNER JOIN [RegisterUZ].[Reporting].[FinancialStatement] AS fs
        ON fs.[RegisterUzEntityId] = ae.[RegisterUzEntityId]
    INNER JOIN [RegisterUZ].[Reporting].[FinancialReport] AS fr
        ON fr.[RegisterUzStatementId] = fs.[RegisterUzStatementId]
    INNER JOIN [Template].[Templates] AS t
        ON CONVERT(bigint, t.[ErpId]) = fr.[RegisterUzTemplateId]
    INNER JOIN [Accounts].[CalculationTemplate] AS ct
        ON ct.[TemplateId] = t.[Id]
    INNER JOIN [Accounts].[AccountFramework] AS af
        ON af.[Id] = ct.[AccountFrameworkId]
    WHERE @FiscalYear BETWEEN 1 AND 9999
      AND ae.[Ico] = @Ico
      AND ae.[IsDeleted] = 0
      AND fs.[IsDeleted] = 0
      AND fr.[IsDeleted] = 0
      AND fs.[PeriodFrom] =
          CONVERT(char(4), @FiscalYear) + '-01'
      AND fs.[PeriodTo] =
          CONVERT(char(4), @FiscalYear) + '-12'
      AND fs.[StatementType] = N'Riadna'
      AND fr.[DataAvailability] = N'Verejné'
      AND EXISTS
      (
          SELECT 1
          FROM [RegisterUZ].[Reporting].[FinancialReportTable] AS frt
          WHERE frt.[RegisterUzFinancialReportId] =
                fr.[RegisterUzFinancialReportId]
      )
);
GO

PRINT 'Calculation-report candidate function created.';
GO
