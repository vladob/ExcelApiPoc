USE [AuditAddIn];
GO

IF (SELECT COUNT_BIG(*) FROM [Accounts].[GetApplicableTemplateFrameworkVersions](690, '2024-12-31')) <> 1
    THROW 52100, 'Template 690 must resolve to exactly one calculation package for 2024.', 1;

IF NOT EXISTS
(
    SELECT 1
    FROM [Accounts].[GetApplicableTemplateFrameworkVersions](690, '2024-12-31')
    WHERE [FrameworkCode] = N'GOV_LOCAL'
      AND [CalculationConfigurationCode] = N'GOV_LOCAL-2023-01'
)
    THROW 52101, 'Template 690 did not resolve to its accepted V1.0 configuration.', 1;

IF EXISTS
(
    SELECT 1
    FROM [Accounts].[GetApplicableTemplateFrameworkVersions](-1, '2024-12-31')
)
    THROW 52102, 'An unsupported template unexpectedly resolved.', 1;

PRINT 'Framework-aware calculation-package validation completed.';
GO
