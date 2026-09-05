/*
    Validates the initial calculation-template mapping.

    This script is non-mutating. CalculationImplemented = 1 is accepted only
    when exactly one internally consistent calculation package exists.
*/

USE [AuditAddIn];
GO

IF OBJECT_ID(N'[Accounts].[CalculationTemplate]', N'U') IS NULL
    THROW 52120, 'Accounts.CalculationTemplate does not exist.', 1;

IF (SELECT COUNT_BIG(*) FROM [Accounts].[CalculationTemplate]) <> 10
    THROW 52121, 'The initial calculation-template mapping must contain exactly ten templates.', 1;

DECLARE @Expected TABLE
(
    [RegisterUzTemplateId] int NOT NULL,
    [FrameworkCode] nvarchar(20) NOT NULL,
    [CalculationImplemented] bit NOT NULL,
    PRIMARY KEY ([RegisterUzTemplateId])
);

INSERT INTO @Expected
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
    SELECT
        e.[RegisterUzTemplateId],
        e.[FrameworkCode],
        e.[CalculationImplemented]
    FROM @Expected AS e

    EXCEPT

    SELECT
        t.[ErpId],
        af.[Code],
        ct.[CalculationImplemented]
    FROM [Accounts].[CalculationTemplate] AS ct
    INNER JOIN [Template].[Templates] AS t
        ON t.[Id] = ct.[TemplateId]
    INNER JOIN [Accounts].[AccountFramework] AS af
        ON af.[Id] = ct.[AccountFrameworkId]
)
OR EXISTS
(
    SELECT
        t.[ErpId],
        af.[Code],
        ct.[CalculationImplemented]
    FROM [Accounts].[CalculationTemplate] AS ct
    INNER JOIN [Template].[Templates] AS t
        ON t.[Id] = ct.[TemplateId]
    INNER JOIN [Accounts].[AccountFramework] AS af
        ON af.[Id] = ct.[AccountFrameworkId]

    EXCEPT

    SELECT
        e.[RegisterUzTemplateId],
        e.[FrameworkCode],
        e.[CalculationImplemented]
    FROM @Expected AS e
)
    THROW 52122, 'The calculation-template mapping differs from the reviewed initial mapping.', 1;

IF
(
    SELECT COUNT_BIG(*)
    FROM [Accounts].[CalculationTemplate]
    WHERE [CalculationImplemented] = 1
) <> 1
    THROW 52123, 'Exactly one template must initially be enabled for calculation.', 1;

IF NOT EXISTS
(
    SELECT 1
    FROM [Accounts].[CalculationTemplate] AS ct
    INNER JOIN [Template].[Templates] AS t
        ON t.[Id] = ct.[TemplateId]
    WHERE t.[ErpId] = 690
      AND ct.[CalculationImplemented] = 1
)
    THROW 52124, 'RegisterUZ template 690 must be the initially enabled calculation template.', 1;

IF EXISTS
(
    SELECT
        ct.[TemplateId]
    FROM [Accounts].[CalculationTemplate] AS ct
    LEFT JOIN [Accounts].[TemplateFrameworkVersion] AS tfv
        ON tfv.[TemplateId] = ct.[TemplateId]
    LEFT JOIN [Accounts].[AccountFrameworkVersion] AS afv
        ON afv.[Id] = tfv.[AccountFrameworkVersionId]
    LEFT JOIN [Accounts].[CalculationConfigurationVersion] AS ccv
        ON ccv.[Id] = tfv.[CalculationConfigurationVersionId]
    WHERE ct.[CalculationImplemented] = 1
    GROUP BY
        ct.[TemplateId],
        ct.[AccountFrameworkId]
    HAVING COUNT(tfv.[Id]) <> 1
        OR COUNT(afv.[Id]) <> 1
        OR COUNT(ccv.[Id]) <> 1
        OR MIN(afv.[AccountFrameworkId]) <> ct.[AccountFrameworkId]
        OR MIN(ccv.[AccountFrameworkVersionId]) <> MIN(tfv.[AccountFrameworkVersionId])
)
    THROW 52125, 'An enabled calculation template does not resolve to exactly one internally consistent calculation package.', 1;

PRINT 'Calculation-template mapping validation completed.';
GO
