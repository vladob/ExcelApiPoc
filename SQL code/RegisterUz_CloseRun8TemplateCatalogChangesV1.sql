/*
    Administratively closes the 65 template-structure changes created by
    sync run 8 after their successful publication to AuditAddIn.

    Preview is the default. The run-5 baseline records are intentionally
    excluded because RequiresReview = 0 for those observations.
*/

USE [RegisterUZ];
GO

SET NOCOUNT OFF;
SET XACT_ABORT ON;

DECLARE @ApplyChanges bit = 0;
DECLARE @Actor nvarchar(200) = N'Vlado Bošnjaković';
DECLARE @SyncRunId bigint = 8;
DECLARE @ExpectedChangeCount bigint = 65;

IF DB_ID(N'AuditAddIn') IS NULL
    THROW 51401, 'Database AuditAddIn was not found.', 1;

DROP TABLE IF EXISTS #TargetChanges;

SELECT
    c.[CatalogChangeId], c.[SyncRunId], c.[CatalogCode],
    c.[SourceObjectKey], c.[ChangeType], c.[ChangeScope],
    c.[ChangeDescription]
INTO #TargetChanges
FROM [Sync].[CatalogChange] c
WHERE c.[SyncRunId] = @SyncRunId
  AND c.[CatalogCode] = 'Templates'
  AND c.[ChangeType] = 'Updated'
  AND c.[ChangeScope] = 'Structure'
  AND c.[RequiresReview] = 1
  AND c.[ReviewedAtUtc] IS NULL
  AND c.[ReviewedBy] IS NULL
  AND c.[PublishedToAuditAddInAtUtc] IS NULL
  AND c.[PublishedBy] IS NULL;

IF (SELECT COUNT_BIG(*) FROM #TargetChanges) <> @ExpectedChangeCount
    THROW 51402, 'Expected exactly 65 pending run-8 template-structure changes.', 1;

IF (SELECT MIN([CatalogChangeId]) FROM #TargetChanges) <> 1132
   OR (SELECT MAX([CatalogChangeId]) FROM #TargetChanges) <> 1196
    THROW 51403, 'The pending CatalogChangeId range is not the verified 1132-1196 range.', 1;

IF EXISTS
(
    SELECT 1
    FROM #TargetChanges c
    LEFT JOIN [Templates].[FinancialReportTemplate] r
      ON r.[RegisterUzTemplateId] = TRY_CONVERT(bigint, c.[SourceObjectKey])
    LEFT JOIN [AuditAddIn].[Template].[Templates] a
      ON CONVERT(bigint, a.[ErpId]) = TRY_CONVERT(bigint, c.[SourceObjectKey])
    WHERE r.[RegisterUzTemplateId] IS NULL OR a.[Id] IS NULL
)
    THROW 51404, 'A target template is missing from RegisterUZ or AuditAddIn.', 1;

/* Reconfirm the structure fields affected by the importer-model correction. */
IF EXISTS
(
    SELECT 1
    FROM #TargetChanges c
    JOIN [Templates].[TemplateTable] r
      ON r.[RegisterUzTemplateId] = TRY_CONVERT(bigint, c.[SourceObjectKey])
    LEFT JOIN [AuditAddIn].[Template].[Tables] a
      ON a.[TemplateErpId] = r.[RegisterUzTemplateId]
     AND a.[TableOrdinal] = r.[TableOrdinal]
    WHERE a.[Id] IS NULL
       OR ISNULL(a.[NumberOfColumns], -1) <> ISNULL(r.[NumberOfColumns], -1)
       OR ISNULL(a.[NumberOfDataColumns], -1) <> ISNULL(r.[NumberOfDataColumns], -1)
)
    THROW 51405, 'A target template table is missing or its canonical column counts differ.', 1;

/* No additional review-required changes may be hidden outside this target. */
IF EXISTS
(
    SELECT 1
    FROM [Sync].[CatalogChange] c
    WHERE c.[RequiresReview] = 1
      AND c.[ReviewedAtUtc] IS NULL
      AND NOT EXISTS
          (SELECT 1 FROM #TargetChanges t
           WHERE t.[CatalogChangeId] = c.[CatalogChangeId])
)
    THROW 51406, 'Another pending review-required catalog change exists.', 1;

SELECT
    COUNT_BIG(*) AS [ChangesToClose],
    MIN([CatalogChangeId]) AS [MinimumCatalogChangeId],
    MAX([CatalogChangeId]) AS [MaximumCatalogChangeId],
    MIN(CONVERT(bigint, [SourceObjectKey])) AS [MinimumTemplateId],
    MAX(CONVERT(bigint, [SourceObjectKey])) AS [MaximumTemplateId],
    @Actor AS [ReviewedAndPublishedBy]
FROM #TargetChanges;

SELECT
    [CatalogChangeId], [SyncRunId], [SourceObjectKey],
    [ChangeType], [ChangeScope], [ChangeDescription]
FROM #TargetChanges
ORDER BY [CatalogChangeId];

IF @ApplyChanges = 0
BEGIN
    PRINT 'PREVIEW ONLY. No persistent changes were made.';
    RETURN;
END;

BEGIN TRY
    BEGIN TRANSACTION;

    DECLARE @ClosedAtUtc datetime2(3) = SYSUTCDATETIME();

    UPDATE c
    SET c.[ReviewedAtUtc] = @ClosedAtUtc,
        c.[ReviewedBy] = @Actor,
        c.[PublishedToAuditAddInAtUtc] = @ClosedAtUtc,
        c.[PublishedBy] = @Actor
    FROM [Sync].[CatalogChange] c
    JOIN #TargetChanges t
      ON t.[CatalogChangeId] = c.[CatalogChangeId]
    WHERE c.[ReviewedAtUtc] IS NULL
      AND c.[ReviewedBy] IS NULL
      AND c.[PublishedToAuditAddInAtUtc] IS NULL
      AND c.[PublishedBy] IS NULL;

    IF @@ROWCOUNT <> @ExpectedChangeCount
        THROW 51407, 'The number of closed changes was not exactly 65.', 1;

    IF EXISTS
    (
        SELECT 1
        FROM #TargetChanges t
        JOIN [Sync].[CatalogChange] c
          ON c.[CatalogChangeId] = t.[CatalogChangeId]
        WHERE c.[ReviewedAtUtc] <> @ClosedAtUtc
           OR c.[ReviewedBy] <> @Actor
           OR c.[PublishedToAuditAddInAtUtc] <> @ClosedAtUtc
           OR c.[PublishedBy] <> @Actor
    )
        THROW 51408, 'Post-update catalog-change verification failed.', 1;

    COMMIT TRANSACTION;

    SELECT
        @ExpectedChangeCount AS [ChangesClosed],
        @ClosedAtUtc AS [ReviewedAndPublishedAtUtc],
        @Actor AS [ReviewedAndPublishedBy];

    PRINT 'Run-8 template catalog changes were reviewed and marked as published to AuditAddIn.';
END TRY
BEGIN CATCH
    IF XACT_STATE() <> 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
