/*
    Adds the reviewed list of individual RegisterUZ financial-report templates
    that are intended for calculation.

    CalculationImplemented is a production-readiness switch. A value of 1 means
    the complete template calculation has been implemented, tested, deployed
    and accepted. It does not replace calculation-configuration validation.
*/

USE [AuditAddIn];
GO

SET NOCOUNT ON;
SET XACT_ABORT ON;
GO

BEGIN TRANSACTION;

IF OBJECT_ID(N'[Accounts].[CalculationTemplate]', N'U') IS NULL
BEGIN
    CREATE TABLE [Accounts].[CalculationTemplate]
    (
        [TemplateId] int NOT NULL,
        [AccountFrameworkId] int NOT NULL,
        [CalculationImplemented] bit NOT NULL
            CONSTRAINT [DF_Accounts_CalculationTemplate_CalculationImplemented]
            DEFAULT (0),

        CONSTRAINT [PK_Accounts_CalculationTemplate]
            PRIMARY KEY CLUSTERED ([TemplateId]),

        CONSTRAINT [FK_Accounts_CalculationTemplate_Template]
            FOREIGN KEY ([TemplateId])
            REFERENCES [Template].[Templates] ([Id]),

        CONSTRAINT [FK_Accounts_CalculationTemplate_AccountFramework]
            FOREIGN KEY ([AccountFrameworkId])
            REFERENCES [Accounts].[AccountFramework] ([Id])
    );
END;

DECLARE @NewData TABLE
(
    [RegisterUzTemplateId] int NOT NULL PRIMARY KEY,
    [FrameworkCode] nvarchar(20) NOT NULL,
    [CalculationImplemented] bit NOT NULL
);

INSERT INTO @NewData
(
    [RegisterUzTemplateId],
    [FrameworkCode],
    [CalculationImplemented]
)
VALUES
    (2,    N'GOV_LOCAL', 0),
    (522,  N'GOV_LOCAL', 0),
    (690,  N'GOV_LOCAL', 1),
    (20,   N'PROFIT',    0),
    (21,   N'PROFIT',    0),
    (687,  N'PROFIT',    0),
    (699,  N'PROFIT',    0),
    (17,   N'NONPROFIT', 0),
    (385,  N'NONPROFIT', 0),
    (1180, N'NONPROFIT', 0);

IF EXISTS
(
    SELECT 1
    FROM @NewData AS n
    LEFT JOIN [Template].[Templates] AS t
        ON t.[ErpId] = n.[RegisterUzTemplateId]
    WHERE t.[Id] IS NULL
)
    THROW 52110, 'A configured RegisterUZ calculation template is missing from Template.Templates.', 1;

IF EXISTS
(
    SELECT 1
    FROM @NewData AS n
    LEFT JOIN [Accounts].[AccountFramework] AS af
        ON af.[Code] = n.[FrameworkCode]
    WHERE af.[Id] IS NULL
)
    THROW 52111, 'A configured calculation-template framework is missing from Accounts.AccountFramework.', 1;

INSERT INTO [Accounts].[CalculationTemplate]
(
    [TemplateId],
    [AccountFrameworkId],
    [CalculationImplemented]
)
SELECT
    t.[Id],
    af.[Id],
    n.[CalculationImplemented]
FROM @NewData AS n
INNER JOIN [Template].[Templates] AS t
    ON t.[ErpId] = n.[RegisterUzTemplateId]
INNER JOIN [Accounts].[AccountFramework] AS af
    ON af.[Code] = n.[FrameworkCode]
LEFT JOIN [Accounts].[CalculationTemplate] AS e
    ON e.[TemplateId] = t.[Id]
WHERE e.[TemplateId] IS NULL;

COMMIT TRANSACTION;

PRINT 'Calculation-template mapping migration completed.';
GO
