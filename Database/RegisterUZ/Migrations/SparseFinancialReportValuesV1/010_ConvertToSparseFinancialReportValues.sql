/*
    Convert normalized financial-report values from a dense representation to
    a sparse one. Canonical source JSON remains unchanged in Raw.PayloadVersion.

    Preview is the default. Set @ApplyChanges to 1 only after reviewing counts.
*/

USE [RegisterUZ];
GO

SET NOCOUNT OFF;
SET XACT_ABORT ON;

DECLARE @ApplyChanges bit = 0;

IF EXISTS
(
    SELECT 1
    FROM [Reporting].[FinancialReportValue]
    WHERE [NumericValue] IS NULL
      AND NULLIF(LTRIM(RTRIM([SourceValue])), N'') IS NOT NULL
)
    THROW 51601, 'A NULL numeric value has nonblank source text; sparse conversion was blocked.', 1;

SELECT
    COUNT_BIG(*) AS [RowsBefore],
    SUM(CASE WHEN [NumericValue] IS NULL THEN CONVERT(bigint, 1) ELSE 0 END)
        AS [BlankRowsToRemove],
    SUM(CASE WHEN [NumericValue] = 0 THEN CONVERT(bigint, 1) ELSE 0 END)
        AS [ExplicitZeroRowsToRetain],
    SUM(CASE WHEN [NumericValue] <> 0 THEN CONVERT(bigint, 1) ELSE 0 END)
        AS [NonZeroRowsToRetain]
FROM [Reporting].[FinancialReportValue];

IF @ApplyChanges = 0
BEGIN
    PRINT 'PREVIEW ONLY. No persistent changes were made.';
    RETURN;
END;

BEGIN TRANSACTION;

DELETE FROM [Reporting].[FinancialReportValue]
WHERE [NumericValue] IS NULL;

DECLARE @RemovedRows bigint = @@ROWCOUNT;

ALTER TABLE [Reporting].[FinancialReportValue]
    ALTER COLUMN [NumericValue] decimal(38,10) NOT NULL;

ALTER TABLE [Reporting].[FinancialReportValue]
    ALTER COLUMN [SourceValue] nvarchar(100) NOT NULL;

IF OBJECT_ID(
       N'[Reporting].[CK_Reporting_FinancialReportValue_SourceNotBlank]',
       N'C') IS NULL
BEGIN
    ALTER TABLE [Reporting].[FinancialReportValue] WITH CHECK
        ADD CONSTRAINT [CK_Reporting_FinancialReportValue_SourceNotBlank]
        CHECK (NULLIF(LTRIM(RTRIM([SourceValue])), N'') IS NOT NULL);
END;

COMMIT TRANSACTION;

SELECT
    @RemovedRows AS [RemovedBlankRows],
    COUNT_BIG(*) AS [RowsAfter],
    SUM(CASE WHEN [NumericValue] = 0 THEN CONVERT(bigint, 1) ELSE 0 END)
        AS [ExplicitZeroRowsRetained],
    SUM(CASE WHEN [NumericValue] <> 0 THEN CONVERT(bigint, 1) ELSE 0 END)
        AS [NonZeroRowsRetained]
FROM [Reporting].[FinancialReportValue];

PRINT 'RegisterUZ financial-report values converted to sparse storage.';
