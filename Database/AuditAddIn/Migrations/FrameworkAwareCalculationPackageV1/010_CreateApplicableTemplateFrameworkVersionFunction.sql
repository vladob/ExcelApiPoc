USE [AuditAddIn];
GO

CREATE OR ALTER FUNCTION [Accounts].[GetApplicableTemplateFrameworkVersions]
(
    @RegisterUzTemplateId int,
    @ApplicableDate date
)
RETURNS TABLE
AS
RETURN
(
    SELECT
        t.[Id] AS [TemplateId],
        t.[ErpId] AS [RegisterUzTemplateId],
        tfv.[Id] AS [TemplateFrameworkVersionId],
        af.[Id] AS [AccountFrameworkId],
        af.[Code] AS [FrameworkCode],
        afv.[Id] AS [AccountFrameworkVersionId],
        afv.[VersionCode] AS [FrameworkVersionCode],
        ccv.[Id] AS [CalculationConfigurationVersionId],
        ccv.[Code] AS [CalculationConfigurationCode],
        ccv.[AccountingModelCode]
    FROM [Template].[Templates] AS t
    INNER JOIN [Accounts].[TemplateFrameworkVersion] AS tfv
        ON tfv.[TemplateId] = t.[Id]
    INNER JOIN [Accounts].[AccountFrameworkVersion] AS afv
        ON afv.[Id] = tfv.[AccountFrameworkVersionId]
    INNER JOIN [Accounts].[AccountFramework] AS af
        ON af.[Id] = afv.[AccountFrameworkId]
    INNER JOIN [Accounts].[CalculationConfigurationVersion] AS ccv
        ON ccv.[Id] = tfv.[CalculationConfigurationVersionId]
    WHERE t.[ErpId] = @RegisterUzTemplateId
      AND (t.[ValidFrom] IS NULL OR t.[ValidFrom] <= @ApplicableDate)
      AND (t.[ValidTo] IS NULL OR t.[ValidTo] >= @ApplicableDate)
      AND afv.[ValidFrom] <= @ApplicableDate
      AND (afv.[ValidTo] IS NULL OR afv.[ValidTo] >= @ApplicableDate)
      AND ccv.[ValidFrom] <= @ApplicableDate
      AND (ccv.[ValidTo] IS NULL OR ccv.[ValidTo] >= @ApplicableDate)
      AND ccv.[AccountFrameworkVersionId] = tfv.[AccountFrameworkVersionId]
);
GO
