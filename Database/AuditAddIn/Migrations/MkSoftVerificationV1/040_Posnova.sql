-- Prerequisite: POSNOVA synchronized in MkSoftRaw, Data.POSNOVA(client, year) available.
USE [AuditAddIn];
GO
CREATE OR ALTER FUNCTION [MkSoft].[AccountingFramework](@ClientErpId int, @FiscalYear int)
RETURNS TABLE AS RETURN
(
    SELECT UCET, NAZOV FROM [MkSoftRaw].[Data].[POSNOVA](@ClientErpId, @FiscalYear)
);
GO
GRANT SELECT ON OBJECT::[MkSoft].[AccountingFramework] TO [IIS APPPOOL\ExcelApiPoc.Api];
GO
USE [MkSoftRaw];
GO
GRANT SELECT ON OBJECT::[Data].[POSNOVA] TO [IIS APPPOOL\ExcelApiPoc.Api];
-- Cross-database access through the TVF may require the underlying table grant.
GRANT SELECT ON OBJECT::[Raw].[POSNOVA] TO [IIS APPPOOL\ExcelApiPoc.Api];
GO
