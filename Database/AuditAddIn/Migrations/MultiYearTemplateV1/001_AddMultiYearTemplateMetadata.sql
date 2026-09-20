USE [AuditAddIn];
GO
SET XACT_ABORT ON;
GO
BEGIN TRANSACTION;
IF COL_LENGTH(N'[Template].[Templates]',N'CreateMultiYear') IS NULL
    ALTER TABLE [Template].[Templates] ADD [CreateMultiYear] bit NOT NULL CONSTRAINT [DF_Template_Templates_CreateMultiYear] DEFAULT(0);
IF COL_LENGTH(N'[Template].[Templates]',N'MultiYearWorksheetName') IS NULL
    ALTER TABLE [Template].[Templates] ADD [MultiYearWorksheetName] nvarchar(31) NULL;
UPDATE [Template].[Templates] SET [CreateMultiYear]=1,[MultiYearWorksheetName]=N'Multi-year Income Statement' WHERE [ErpId] IN (1,7,10,12,13,14,18,19,22,27,30,61,62,521,696,727,1142);
UPDATE [Template].[Templates] SET [CreateMultiYear]=1,[MultiYearWorksheetName]=N'Multi-year Balance Sheet' WHERE [ErpId] IN (2,3,9,11,17,20,21,29,522,541,542,684,690,1141);
UPDATE [Template].[Templates] SET [CreateMultiYear]=1,[MultiYearWorksheetName]=N'Multi-year Balance & Income' WHERE [ErpId] IN (662,663,687,699,711,723,738,801,941,1001,1021,1101,1121,1180,5181,5184);
IF NOT EXISTS(SELECT 1 FROM sys.check_constraints WHERE [name]=N'CK_Template_Templates_MultiYear' AND [parent_object_id]=OBJECT_ID(N'[Template].[Templates]'))
    ALTER TABLE [Template].[Templates] WITH CHECK ADD CONSTRAINT [CK_Template_Templates_MultiYear] CHECK ([CreateMultiYear]=0 OR NULLIF(LTRIM(RTRIM([MultiYearWorksheetName])),N'') IS NOT NULL);
COMMIT TRANSACTION;
GO
