-- Deploy before the 0.1.12 API. Existing result columns remain compatible.
USE [AuditAddIn];
GO
CREATE OR ALTER FUNCTION [MkSoft].[EntityInfo](@ClientErpId int, @FiscalYear int)
RETURNS TABLE AS RETURN
(
    SELECT @ClientErpId AS ClientErpId, u.ICO, u.NAZOV1, u.PRAVNAFORMA,
        CONCAT_WS(N', ', NULLIF(LTRIM(RTRIM(u.ULICA)), N''),
            NULLIF(LTRIM(RTRIM(CONCAT_WS(N' ', NULLIF(LTRIM(RTRIM(u.PSC)), N''),
                NULLIF(LTRIM(RTRIM(u.OBEC)), N'')))), N'')) AS EntityAddress
    FROM [MkSoftRaw].[Data].[UDAJEFIRMY](@ClientErpId, @FiscalYear) u
);
GO
CREATE OR ALTER FUNCTION [MkSoft].[EntitiesByIco](@Ico int, @FiscalYear int)
RETURNS TABLE AS RETURN
(
    SELECT u.ClientErpId, u.ICO, u.NAZOV1, u.PRAVNAFORMA,
        CONCAT_WS(N', ', NULLIF(LTRIM(RTRIM(u.ULICA)), N''),
            NULLIF(LTRIM(RTRIM(CONCAT_WS(N' ', NULLIF(LTRIM(RTRIM(u.PSC)), N''),
                NULLIF(LTRIM(RTRIM(u.OBEC)), N'')))), N'')) AS EntityAddress
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

USE [AuditAddIn];
GO
CREATE OR ALTER FUNCTION [MkSoft].[GeneralLedger]
(@ClientErpId int, @FiscalYear int, @IncludeEmptyAccounts bit)
RETURNS TABLE AS RETURN
(
    SELECT gl.AccountNo, gl.AccountName, gl.OpeningDebit, gl.OpeningCredit,
           gl.MovementsDebit, gl.MovementsCredit, gl.ClosingDebit, gl.ClosingCredit
    FROM [MkSoftRaw].[Data].[GeneralLedger](@ClientErpId, @FiscalYear) gl
    WHERE LEFT(gl.AccountNo, 1) IN ('0','1','2','3','4','5','6','7')
      AND (@IncludeEmptyAccounts = 1
        OR gl.OpeningDebit - gl.OpeningCredit <> 0
        OR gl.MovementsDebit <> 0 OR gl.MovementsCredit <> 0
        OR gl.ClosingDebit - gl.ClosingCredit <> 0)
);
GO
