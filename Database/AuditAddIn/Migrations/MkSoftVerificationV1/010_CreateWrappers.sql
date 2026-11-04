USE [AuditAddIn];
GO
IF SCHEMA_ID(N'MkSoft') IS NULL EXEC(N'CREATE SCHEMA [MkSoft] AUTHORIZATION [dbo]');
GO
CREATE OR ALTER FUNCTION [MkSoft].[GeneralLedger]
(@ClientErpId int, @FiscalYear int, @IncludeEmptyAccounts bit)
RETURNS TABLE AS RETURN
(
    SELECT gl.AccountNo, gl.AccountName, gl.OpeningDebit, gl.OpeningCredit,
           gl.MovementsDebit, gl.MovementsCredit, gl.ClosingDebit, gl.ClosingCredit
    FROM [MkSoftRaw].[Data].[GeneralLedger](@ClientErpId, @FiscalYear) gl
    WHERE LEFT(gl.AccountNo, 1) IN ('0','1','2','3','4')
      AND (@IncludeEmptyAccounts = 1
        OR gl.OpeningDebit - gl.OpeningCredit <> 0
        OR gl.MovementsDebit <> 0 OR gl.MovementsCredit <> 0
        OR gl.ClosingDebit - gl.ClosingCredit <> 0)
);
GO
CREATE OR ALTER FUNCTION [MkSoft].[EntityLegalForm](@ClientErpId int, @FiscalYear int)
RETURNS TABLE AS RETURN
(
    SELECT PRAVNAFORMA FROM [MkSoftRaw].[Data].[UDAJEFIRMY](@ClientErpId, @FiscalYear)
);
GO
