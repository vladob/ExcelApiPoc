/* Run after 010-030. The IIS identity used by ExcelApiPoc.Api must already exist. */
USE [AuditAddIn];
GO

IF DATABASE_PRINCIPAL_ID(N'IIS APPPOOL\ExcelApiPoc.Api') IS NULL
    THROW 53400, 'The ExcelApiPoc.Api database user does not exist.', 1;
GO

GRANT SELECT ON OBJECT::[Settings].[Users] TO [IIS APPPOOL\ExcelApiPoc.Api];
GRANT SELECT ON OBJECT::[Settings].[TextCategories] TO [IIS APPPOOL\ExcelApiPoc.Api];
GRANT SELECT, INSERT, UPDATE, DELETE ON OBJECT::[Settings].[PredefinedTexts] TO [IIS APPPOOL\ExcelApiPoc.Api];
GRANT SELECT, INSERT, UPDATE, DELETE ON OBJECT::[Settings].[PredefinedTextMappings] TO [IIS APPPOOL\ExcelApiPoc.Api];
GRANT SELECT ON OBJECT::[Settings].[WorksheetLayouts] TO [IIS APPPOOL\ExcelApiPoc.Api];
GRANT SELECT ON OBJECT::[Settings].[WorksheetLayoutVersions] TO [IIS APPPOOL\ExcelApiPoc.Api];
GRANT SELECT ON OBJECT::[Settings].[WorksheetLayoutDefaults] TO [IIS APPPOOL\ExcelApiPoc.Api];
GRANT SELECT ON OBJECT::[Settings].[UserWorksheetLayouts] TO [IIS APPPOOL\ExcelApiPoc.Api];
GO
