USE [AuditAddIn];
GO
CREATE OR ALTER FUNCTION [MkSoft].[EntityInfo](@ClientErpId int, @FiscalYear int)
RETURNS TABLE AS RETURN
(
    SELECT @ClientErpId AS ClientErpId, u.ICO, u.NAZOV1, u.PRAVNAFORMA
    FROM [MkSoftRaw].[Data].[UDAJEFIRMY](@ClientErpId, @FiscalYear) u
);
GO
CREATE OR ALTER FUNCTION [MkSoft].[EntitiesByIco](@Ico int, @FiscalYear int)
RETURNS TABLE AS RETURN
(
    SELECT u.ClientErpId, u.ICO, u.NAZOV1, u.PRAVNAFORMA
    FROM [MkSoftRaw].[Raw].[UDAJEFIRMY] u
    WHERE u.ValidTo IS NULL AND u.FiscalYear = @FiscalYear AND u.ICO = @Ico
      AND EXISTS (SELECT 1 FROM [MkSoftRaw].[Master].[MkSoftClients] c
                  WHERE c.ErpId = u.ClientErpId AND (c.Agendas LIKE '%P%' OR c.Agendas LIKE '%J%'))
);
GO
-- Existing production API identity; adapt if your IIS app uses another identity.
GRANT SELECT ON OBJECT::[MkSoft].[EntityInfo] TO [IIS APPPOOL\ExcelApiPoc.Api];
GRANT SELECT ON OBJECT::[MkSoft].[EntitiesByIco] TO [IIS APPPOOL\ExcelApiPoc.Api];
GO
USE [MkSoftRaw];
GO
GRANT SELECT ON OBJECT::[Data].[UDAJEFIRMY] TO [IIS APPPOOL\ExcelApiPoc.Api];
GRANT SELECT ON OBJECT::[Raw].[UDAJEFIRMY] TO [IIS APPPOOL\ExcelApiPoc.Api];
GRANT SELECT ON OBJECT::[Master].[MkSoftClients] TO [IIS APPPOOL\ExcelApiPoc.Api];
GO
