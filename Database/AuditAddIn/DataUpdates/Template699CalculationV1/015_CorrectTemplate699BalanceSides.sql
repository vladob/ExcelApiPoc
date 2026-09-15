/* Correct dual-sided balances discovered during Template 699 reconciliation. */
USE [AuditAddIn];
GO

SET NOCOUNT ON;
SET XACT_ABORT ON;
GO

BEGIN TRANSACTION;

DECLARE @FrameworkVersionId int =
(
    SELECT afv.[Id]
    FROM [Accounts].[AccountFrameworkVersion] afv
    INNER JOIN [Accounts].[AccountFramework] af
        ON af.[Id] = afv.[AccountFrameworkId]
    WHERE af.[Code] = N'PROFIT'
      AND afv.[VersionCode] = N'2022-01-01'
);

DECLARE @ConfigurationId int =
(
    SELECT [Id]
    FROM [Accounts].[CalculationConfigurationVersion]
    WHERE [AccountFrameworkVersionId] = @FrameworkVersionId
      AND [Code] = N'PROFIT-2022-01'
);

DECLARE @TemplateId int =
    (SELECT [Id] FROM [Template].[Templates] WHERE [ErpId] = 699);

DECLARE @TemplateFrameworkVersionId int =
(
    SELECT [Id]
    FROM [Accounts].[TemplateFrameworkVersion]
    WHERE [TemplateId] = @TemplateId
      AND [CalculationConfigurationVersionId] = @ConfigurationId
);

IF @FrameworkVersionId IS NULL OR @ConfigurationId IS NULL
   OR @TemplateId IS NULL OR @TemplateFrameworkVersionId IS NULL
    THROW 52340, 'Template 699 PROFIT calculation configuration is missing.', 1;

UPDATE acr
SET acr.[LiabilitiesValueSourceCode] = N'ClosingCredit'
FROM [Accounts].[AccountCalculationRules] acr
INNER JOIN [Accounts].[Accounts] a ON a.[Id] = acr.[AccountId]
WHERE acr.[CalculationConfigurationVersionId] = @ConfigurationId
  AND a.[AccountCode] IN
      (N'341', N'342', N'343', N'345', N'346', N'347');

IF @@ROWCOUNT <> 6
    THROW 52341, 'Expected six Template 699 tax-account rules.', 1;

UPDATE m
SET m.[RequiresAnalyticalMapping] = 1
FROM [Accounts].[ReportAccountMappings] m
INNER JOIN [Template].[Rows] r ON r.[Id] = m.[TemplateRowId]
INNER JOIN [Template].[Tables] tt ON tt.[Id] = r.[TableId]
INNER JOIN [Accounts].[AccountCalculationRules] acr
    ON acr.[Id] = m.[AccountCalculationRuleId]
INNER JOIN [Accounts].[Accounts] a ON a.[Id] = acr.[AccountId]
WHERE m.[TemplateFrameworkVersionId] = @TemplateFrameworkVersionId
  AND a.[AccountCode] = N'221'
  AND ((tt.[TableErpId] = 69901 AND r.[RowNumber] = 73)
    OR (tt.[TableErpId] = 69902 AND r.[RowNumber] = 139));

IF @@ROWCOUNT <> 2
    THROW 52342, 'Expected two Template 699 account-221 mappings.', 1;

UPDATE m
SET m.[ValueSourceCode] = N'ClosingCredit'
FROM [Accounts].[ReportAccountMappings] m
INNER JOIN [Template].[Rows] r ON r.[Id] = m.[TemplateRowId]
INNER JOIN [Template].[Tables] tt ON tt.[Id] = r.[TableId]
INNER JOIN [Accounts].[AccountCalculationRules] acr
    ON acr.[Id] = m.[AccountCalculationRuleId]
INNER JOIN [Accounts].[Accounts] a ON a.[Id] = acr.[AccountId]
WHERE m.[TemplateFrameworkVersionId] = @TemplateFrameworkVersionId
  AND tt.[TableErpId] = 69902
  AND r.[RowNumber] = 133
  AND a.[AccountCode] IN
      (N'341', N'342', N'343', N'345', N'346', N'347');

IF @@ROWCOUNT <> 6
    THROW 52343, 'Expected six Template 699 tax-liability mappings.', 1;

COMMIT TRANSACTION;
PRINT 'Template 699 balance-side correction completed.';
GO
