/* Non-mutating structural validation for the Template 699 calculation package. */
USE [AuditAddIn];
GO

DECLARE @TemplateId int = (SELECT [Id] FROM [Template].[Templates] WHERE [ErpId] = 699);
DECLARE @FrameworkVersionId int =
(
    SELECT afv.[Id]
    FROM [Accounts].[AccountFrameworkVersion] afv
    INNER JOIN [Accounts].[AccountFramework] af ON af.[Id] = afv.[AccountFrameworkId]
    WHERE af.[Code] = N'PROFIT' AND afv.[VersionCode] = N'2022-01-01'
);
DECLARE @ConfigurationId int =
(
    SELECT [Id]
    FROM [Accounts].[CalculationConfigurationVersion]
    WHERE [AccountFrameworkVersionId] = @FrameworkVersionId
      AND [Code] = N'PROFIT-2022-01'
);
DECLARE @TemplateFrameworkVersionId int =
(
    SELECT [Id]
    FROM [Accounts].[TemplateFrameworkVersion]
    WHERE [TemplateId] = @TemplateId
      AND [CalculationConfigurationVersionId] = @ConfigurationId
);

IF @TemplateId IS NULL OR @FrameworkVersionId IS NULL OR @ConfigurationId IS NULL
   OR @TemplateFrameworkVersionId IS NULL
    THROW 52320, 'Template 699 does not resolve to its PROFIT calculation configuration.', 1;

IF (SELECT COUNT_BIG(*) FROM [Accounts].[AccountCalculationRules]
    WHERE [CalculationConfigurationVersionId] = @ConfigurationId) <> 228
    THROW 52321, 'Template 699 must have exactly 228 account calculation rules.', 1;

IF (SELECT COUNT_BIG(*) FROM [Accounts].[ReportAccountMappings]
    WHERE [TemplateFrameworkVersionId] = @TemplateFrameworkVersionId) <> 547
    THROW 52322, 'Template 699 must have exactly 547 report-account mappings.', 1;

IF EXISTS
(
    SELECT 1
    FROM [Accounts].[ReportAccountMappings] m
    INNER JOIN [Template].[Rows] r ON r.[Id] = m.[TemplateRowId]
    INNER JOIN [Template].[Tables] tt ON tt.[Id] = r.[TableId]
    WHERE m.[TemplateFrameworkVersionId] = @TemplateFrameworkVersionId
      AND tt.[TemplateId] <> @TemplateId
)
    THROW 52323, 'A Template 699 mapping references a row from another template.', 1;

IF EXISTS
(
    SELECT 1
    FROM [Accounts].[ReportAccountMappings] m
    INNER JOIN [Accounts].[AccountCalculationRules] r ON r.[Id] = m.[AccountCalculationRuleId]
    WHERE m.[TemplateFrameworkVersionId] = @TemplateFrameworkVersionId
      AND r.[CalculationConfigurationVersionId] <> @ConfigurationId
)
    THROW 52324, 'A Template 699 mapping references another calculation configuration.', 1;

IF
(
    SELECT COUNT_BIG(*)
    FROM [Template].[RowCalculationTerms] term
    INNER JOIN [Template].[Rows] targetRow ON targetRow.[Id] = term.[TargetRowId]
    INNER JOIN [Template].[Tables] targetTable ON targetTable.[Id] = targetRow.[TableId]
    WHERE targetTable.[TemplateId] = @TemplateId
) <> 214
    THROW 52325, 'Template 699 must have exactly 214 direct row-calculation terms.', 1;

IF NOT EXISTS
(
    SELECT 1
    FROM [Template].[RowCalculationTerms] term
    INNER JOIN [Template].[Rows] targetRow ON targetRow.[Id] = term.[TargetRowId]
    INNER JOIN [Template].[Tables] targetTable ON targetTable.[Id] = targetRow.[TableId]
    INNER JOIN [Template].[Rows] sourceRow ON sourceRow.[Id] = term.[SourceRowId]
    INNER JOIN [Template].[Tables] sourceTable ON sourceTable.[Id] = sourceRow.[TableId]
    WHERE targetTable.[TableErpId] = 69902 AND targetRow.[RowNumber] = 100
      AND sourceTable.[TableErpId] = 69903 AND sourceRow.[RowNumber] = 61
      AND term.[Coefficient] = 1
)
    THROW 52326, 'Income-statement row 61 is not linked to balance-sheet row 100.', 1;

IF NOT EXISTS
(
    SELECT 1
    FROM [Accounts].[CalculationTemplate] ct
    INNER JOIN [Accounts].[AccountFrameworkVersion] afv
        ON afv.[AccountFrameworkId] = ct.[AccountFrameworkId]
    WHERE ct.[TemplateId] = @TemplateId
      AND afv.[Id] = @FrameworkVersionId
      AND ct.[CalculationImplemented] = 1
)
    THROW 52327, 'Template 699 is not enabled for calculation.', 1;

IF (SELECT COUNT_BIG(*) FROM [Template].[GetCalculationPlan](699)) = 0
    THROW 52328, 'Template 699 generated calculation plan is empty.', 1;

PRINT 'Template 699 calculation validation completed.';
GO
