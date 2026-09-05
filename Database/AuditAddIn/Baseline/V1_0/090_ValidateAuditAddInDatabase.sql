/* Blocking structural, relationship and current-data validation. Read-only. */
USE [AuditAddIn];
GO

SET NOCOUNT ON;

IF DB_NAME() <> N'AuditAddIn'
    THROW 52900, 'Validation must run in AuditAddIn.', 1;

/* Expected seed cardinalities. */
IF (SELECT COUNT_BIG(*) FROM [Accounts].[AccountFramework]) <> 3
    THROW 52901, 'Expected 3 account frameworks.', 1;
IF (SELECT COUNT_BIG(*) FROM [Accounts].[AccountFrameworkVersion]) <> 3
    THROW 52902, 'Expected 3 account-framework versions.', 1;
IF (SELECT COUNT_BIG(*) FROM [Accounts].[OfficialAccounts]) <> 645
    THROW 52903, 'Expected 645 official accounts.', 1;
IF (SELECT COUNT_BIG(*) FROM [Accounts].[Accounts]) <> 645
    THROW 52904, 'Expected 645 production accounts.', 1;
IF (SELECT COUNT_BIG(*) FROM [Accounts].[AccountRanges]) <> 2
    THROW 52905, 'Expected 2 GOV_LOCAL fallback account ranges.', 1;
IF (SELECT COUNT_BIG(*) FROM [Template].[Templates]) <> 245
    THROW 52906, 'Expected 245 templates.', 1;
IF (SELECT COUNT_BIG(*) FROM [Template].[Tables]) <> 167
    THROW 52907, 'Expected 167 template tables.', 1;
IF (SELECT COUNT_BIG(*) FROM [Template].[Headers]) <> 1856
    THROW 52908, 'Expected 1,856 template headers.', 1;
IF (SELECT COUNT_BIG(*) FROM [Template].[Rows]) <> 6502
    THROW 52909, 'Expected 6,502 template rows.', 1;
IF (SELECT COUNT_BIG(*) FROM [Accounts].[AccountCalculationRules]) <> 315
    THROW 52910, 'Expected 315 GOV_LOCAL calculation rules.', 1;
IF (SELECT COUNT_BIG(*) FROM [Accounts].[CalculationConfigurationVersion]) <> 1
    THROW 52930, 'Expected one calculation-configuration version.', 1;
IF (SELECT COUNT_BIG(*) FROM [Accounts].[TemplateFrameworkVersion]) <> 1
    THROW 52931, 'Expected one template/framework/configuration association.', 1;
IF (SELECT COUNT_BIG(*) FROM [Accounts].[ReportAccountMappings]) <> 267
    THROW 52911, 'Expected 267 template-690 account mappings.', 1;
IF (SELECT COUNT_BIG(*) FROM [Template].[RowCalculationTerms]) <> 188
    THROW 52912, 'Expected 188 normalized direct calculation terms.', 1;

/* One open version and no overlapping effective periods per framework. */
IF EXISTS
(
    SELECT afv.[AccountFrameworkId]
    FROM [Accounts].[AccountFrameworkVersion] afv
    WHERE afv.[ValidTo] IS NULL
    GROUP BY afv.[AccountFrameworkId]
    HAVING COUNT_BIG(*) <> 1
)
    THROW 52913, 'Each populated framework must have exactly one open version.', 1;

IF EXISTS
(
    SELECT 1
    FROM [Accounts].[AccountFrameworkVersion] a
    INNER JOIN [Accounts].[AccountFrameworkVersion] b
        ON b.[AccountFrameworkId] = a.[AccountFrameworkId]
       AND b.[Id] > a.[Id]
       AND b.[ValidFrom] <= ISNULL(a.[ValidTo], CONVERT(date, '9999-12-31'))
       AND a.[ValidFrom] <= ISNULL(b.[ValidTo], CONVERT(date, '9999-12-31'))
)
    THROW 52914, 'Account-framework effective periods overlap.', 1;

/* Production accounts initially mirror the reviewed official source exactly. */
IF EXISTS
(
    SELECT [AccountFrameworkVersionId], [AccountCode], [AccountName_sk], [AccountLevel], [SortOrder]
    FROM [Accounts].[OfficialAccounts]
    EXCEPT
    SELECT [AccountFrameworkVersionId], [AccountCode], [AccountName_sk], [AccountLevel], [SortOrder]
    FROM [Accounts].[Accounts]
)
OR EXISTS
(
    SELECT [AccountFrameworkVersionId], [AccountCode], [AccountName_sk], [AccountLevel], [SortOrder]
    FROM [Accounts].[Accounts]
    EXCEPT
    SELECT [AccountFrameworkVersionId], [AccountCode], [AccountName_sk], [AccountLevel], [SortOrder]
    FROM [Accounts].[OfficialAccounts]
)
    THROW 52915, 'Production and official account values differ.', 1;

IF EXISTS
(
    SELECT 1
    FROM [Accounts].[Accounts] a
    LEFT JOIN [Accounts].[Accounts] p ON p.[Id] = a.[ParentAccountId]
    WHERE a.[ParentAccountId] IS NOT NULL
      AND (p.[AccountFrameworkVersionId] <> a.[AccountFrameworkVersionId]
        OR p.[AccountCode] <> LEFT(a.[AccountCode], LEN(a.[AccountCode]) - 1))
)
    THROW 52916, 'A production account has an invalid parent.', 1;

IF (SELECT COUNT_BIG(*) FROM [Accounts].[GetAccounts](N'GOV_LOCAL', NULL)) <> 344
    THROW 52917, 'Current GOV_LOCAL must return 344 accounts.', 1;
IF (SELECT COUNT_BIG(*) FROM [Accounts].[GetAccounts](N'PROFIT', NULL)) <> 301
    THROW 52918, 'Current PROFIT must return 301 accounts.', 1;
IF (SELECT COUNT_BIG(*) FROM [Accounts].[GetAccounts](N'NONPROFIT', NULL)) <> 0
    THROW 52919, 'Current NONPROFIT must currently return no accounts.', 1;

/* Configuration rows cannot cross their selected framework/configuration boundary. */
IF EXISTS
(
    SELECT 1
    FROM [Accounts].[AccountCalculationRules] r
    INNER JOIN [Accounts].[CalculationConfigurationVersion] ccv ON ccv.[Id] = r.[CalculationConfigurationVersionId]
    INNER JOIN [Accounts].[Accounts] a ON a.[Id] = r.[AccountId]
    WHERE a.[AccountFrameworkVersionId] <> ccv.[AccountFrameworkVersionId]
)
    THROW 52920, 'A calculation rule references an account from another framework version.', 1;

IF EXISTS
(
    SELECT 1
    FROM [Accounts].[TemplateFrameworkVersion] tfv
    INNER JOIN [Accounts].[CalculationConfigurationVersion] ccv ON ccv.[Id] = tfv.[CalculationConfigurationVersionId]
    WHERE ccv.[AccountFrameworkVersionId] <> tfv.[AccountFrameworkVersionId]
)
    THROW 52921, 'A template association crosses framework and configuration versions.', 1;

IF EXISTS
(
    SELECT 1
    FROM [Accounts].[ReportAccountMappings] m
    INNER JOIN [Accounts].[TemplateFrameworkVersion] tfv ON tfv.[Id] = m.[TemplateFrameworkVersionId]
    INNER JOIN [Template].[Rows] tr ON tr.[Id] = m.[TemplateRowId]
    INNER JOIN [Template].[Tables] tt ON tt.[Id] = tr.[TableId]
    INNER JOIN [Accounts].[AccountCalculationRules] r ON r.[Id] = m.[AccountCalculationRuleId]
    WHERE tt.[TemplateId] <> tfv.[TemplateId]
       OR r.[CalculationConfigurationVersionId] <> tfv.[CalculationConfigurationVersionId]
)
    THROW 52922, 'A report mapping crosses its template or calculation configuration.', 1;

IF EXISTS (SELECT 1 FROM [Accounts].[AccountCalculationRules] WHERE [Legend] <> RTRIM([Legend]))
    THROW 52923, 'A calculation-rule legend contains trailing whitespace.', 1;

IF NOT EXISTS
(
    SELECT 1
    FROM [Accounts].[AccountCalculationRuleDetails]
    WHERE [FrameworkCode] = N'GOV_LOCAL' AND [AccountCode] = N'355'
      AND [AssetsValueSourceCode] = N'ClosingDebit' AND [LiabilitiesValueSourceCode] IS NULL
)
    THROW 52924, 'The corrected account 355 calculation rule was not preserved.', 1;

IF NOT EXISTS
(
    SELECT 1
    FROM [Accounts].[TemplateFrameworkVersion] tfv
    INNER JOIN [Template].[Templates] t ON t.[Id] = tfv.[TemplateId]
    INNER JOIN [Accounts].[CalculationConfigurationVersion] ccv ON ccv.[Id] = tfv.[CalculationConfigurationVersionId]
    WHERE t.[ErpId] = 690 AND ccv.[AccountingModelCode] = N'GOV'
)
    THROW 52932, 'Template 690 must preserve accounting model GOV.', 1;

/* Detect a cycle before executing the recursive plan function. */
DECLARE @HasCalculationCycle bit = 0;

;WITH [Paths] AS
(
    SELECT ct.[TargetRowId] AS [RootRowId], ct.[SourceRowId],
           CONVERT(varchar(max), '|' + CONVERT(varchar(20), ct.[TargetRowId]) + '|' + CONVERT(varchar(20), ct.[SourceRowId]) + '|') AS [Path]
    FROM [Template].[RowCalculationTerms] ct
    UNION ALL
    SELECT p.[RootRowId], ct.[SourceRowId],
           CONVERT(varchar(max), p.[Path] + CONVERT(varchar(20), ct.[SourceRowId]) + '|')
    FROM [Paths] p
    INNER JOIN [Template].[RowCalculationTerms] ct ON ct.[TargetRowId] = p.[SourceRowId]
    WHERE CHARINDEX('|' + CONVERT(varchar(20), ct.[SourceRowId]) + '|', p.[Path]) = 0
)
SELECT TOP (1) @HasCalculationCycle = 1
FROM [Paths] p
INNER JOIN [Template].[RowCalculationTerms] ct ON ct.[TargetRowId] = p.[SourceRowId]
WHERE CHARINDEX('|' + CONVERT(varchar(20), ct.[SourceRowId]) + '|', p.[Path]) > 0
OPTION (MAXRECURSION 32767);

IF @HasCalculationCycle = 1
    THROW 52925, 'The row-calculation graph contains a cycle.', 1;

IF (SELECT COUNT_BIG(*) FROM [Template].[GetCalculationPlan](690)) <> 959
    THROW 52926, 'Template 690 must generate the expected 959 calculation-plan rows.', 1;
IF (SELECT COUNT_BIG(*) FROM [Template].[GetCalculationPlan](690) WHERE [Coefficient] = 1) <> 797
    THROW 52927, 'Template 690 positive calculation terms differ from the verified plan.', 1;
IF (SELECT COUNT_BIG(*) FROM [Template].[GetCalculationPlan](690) WHERE [Coefficient] = -1) <> 162
    THROW 52928, 'Template 690 negative calculation terms differ from the verified plan.', 1;
IF (SELECT COUNT_BIG(*) FROM (SELECT [SumTableErpId], [SumRow] FROM [Template].[GetCalculationPlan](690) GROUP BY [SumTableErpId], [SumRow]) x) <> 27
    THROW 52929, 'Template 690 must contain 27 calculated target rows.', 1;

/* Successful summary plus non-blocking source-metadata observations. */
SELECT N'AccountFramework' AS [ObjectName], COUNT_BIG(*) AS [RowCount] FROM [Accounts].[AccountFramework]
UNION ALL SELECT N'AccountFrameworkVersion', COUNT_BIG(*) FROM [Accounts].[AccountFrameworkVersion]
UNION ALL SELECT N'OfficialAccounts', COUNT_BIG(*) FROM [Accounts].[OfficialAccounts]
UNION ALL SELECT N'Accounts', COUNT_BIG(*) FROM [Accounts].[Accounts]
UNION ALL SELECT N'AccountCalculationRules', COUNT_BIG(*) FROM [Accounts].[AccountCalculationRules]
UNION ALL SELECT N'ReportAccountMappings', COUNT_BIG(*) FROM [Accounts].[ReportAccountMappings]
UNION ALL SELECT N'Templates', COUNT_BIG(*) FROM [Template].[Templates]
UNION ALL SELECT N'Tables', COUNT_BIG(*) FROM [Template].[Tables]
UNION ALL SELECT N'Headers', COUNT_BIG(*) FROM [Template].[Headers]
UNION ALL SELECT N'Rows', COUNT_BIG(*) FROM [Template].[Rows]
UNION ALL SELECT N'RowCalculationTerms', COUNT_BIG(*) FROM [Template].[RowCalculationTerms]
UNION ALL SELECT N'GeneratedCalculationPlan(690)', COUNT_BIG(*) FROM [Template].[GetCalculationPlan](690);

SELECT af.[Code] AS [FrameworkCode], afv.[VersionCode], afv.[ValidFrom], afv.[LegalReference], afv.[SourceUrl],
       CASE WHEN afv.[VersionCode] <> CONVERT(char(10), afv.[ValidFrom], 23) THEN N'VersionCode differs from ValidFrom. Review source metadata.' END AS [VersionObservation],
       CASE WHEN afv.[LegalReference] IN (N'Book1', N'Book2') OR afv.[SourceUrl] = N'scan' THEN N'Placeholder provenance should eventually be replaced.' END AS [ProvenanceObservation]
FROM [Accounts].[AccountFrameworkVersion] afv
INNER JOIN [Accounts].[AccountFramework] af ON af.[Id] = afv.[AccountFrameworkId]
WHERE afv.[VersionCode] <> CONVERT(char(10), afv.[ValidFrom], 23)
   OR afv.[LegalReference] IN (N'Book1', N'Book2')
   OR afv.[SourceUrl] = N'scan'
ORDER BY af.[Code];

PRINT 'AuditAddIn validation completed successfully.';
GO
