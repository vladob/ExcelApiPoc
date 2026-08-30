/*
    One-time, administrator-controlled migration of the existing AuditAddIn
    template projection to stable RegisterUZ ordinals and current official data.

    Preview is the default. Change @ApplyChanges to 1 only after reviewing the
    result sets produced on the target databases.

    Identity recovery:
      tables:  TemplateErpId + existing natural table order
      headers: table + RowPosition + ColumnPosition
      ordinary rows: table + unique RowNumber
      legacy NULL-number rows: rebuild 24 unreferenced, unenriched projections

    Local English captions, calculation metadata, categories and mappings are
    never overwritten on rows and headers that are retained.
*/

USE [AuditAddIn];
GO

SET NOCOUNT OFF;
SET XACT_ABORT ON;

DECLARE @ApplyChanges bit = 0;

IF DB_ID(N'RegisterUZ') IS NULL
    THROW 51201, 'Database RegisterUZ was not found.', 1;

IF COL_LENGTH(N'Template.Tables', N'TableOrdinal') IS NOT NULL
   OR COL_LENGTH(N'Template.Headers', N'HeaderOrdinal') IS NOT NULL
   OR COL_LENGTH(N'Template.Rows', N'RowOrdinal') IS NOT NULL
    THROW 51202, 'Ordinal columns already exist. This one-time V2 migration must not be rerun.', 1;

DROP TABLE IF EXISTS #AuditTables;
DROP TABLE IF EXISTS #TableMap;
DROP TABLE IF EXISTS #HeaderMap;
DROP TABLE IF EXISTS #AuditRowKeys;
DROP TABLE IF EXISTS #RegisterUzRowKeys;
DROP TABLE IF EXISTS #OrdinaryRowMap;
DROP TABLE IF EXISTS #AffectedTables;
DROP TABLE IF EXISTS #BlockingDependencies;

/* Table mapping. The prior comparison proved all 167 tables match exactly. */
SELECT
    t.[Id] AS [AuditTableId], t.[TableErpId], t.[TemplateId], t.[TemplateErpId],
    CONVERT(int, ROW_NUMBER() OVER
    (
        PARTITION BY t.[TemplateErpId] ORDER BY t.[Id]
    ) - 1) AS [TableOrdinal]
INTO #AuditTables
FROM [Template].[Tables] t;

SELECT
    a.[AuditTableId], a.[TableErpId], a.[TemplateId], a.[TemplateErpId],
    a.[TableOrdinal], r.[TemplateTableId] AS [RegisterUzTemplateTableId],
    r.[RegisterUzTemplateId]
INTO #TableMap
FROM #AuditTables a
FULL OUTER JOIN [RegisterUZ].[Templates].[TemplateTable] r
  ON r.[RegisterUzTemplateId] = a.[TemplateErpId]
 AND r.[TableOrdinal] = a.[TableOrdinal];

IF (SELECT COUNT_BIG(*) FROM #TableMap) <> 167
   OR EXISTS
      (SELECT 1 FROM #TableMap
       WHERE [AuditTableId] IS NULL OR [RegisterUzTemplateTableId] IS NULL)
    THROW 51203, 'Expected a complete 167-row table mapping.', 1;

/* Header coordinates are unique on both sides and matched all 1,856 rows. */
SELECT
    a.[Id] AS [AuditHeaderId], tm.[AuditTableId],
    r.[TemplateHeaderId] AS [RegisterUzTemplateHeaderId], r.[HeaderOrdinal]
INTO #HeaderMap
FROM #TableMap tm
JOIN [Template].[Headers] a ON a.[TableId] = tm.[AuditTableId]
JOIN [RegisterUZ].[Templates].[TemplateHeader] r
  ON r.[TemplateTableId] = tm.[RegisterUzTemplateTableId]
 AND r.[RowPosition] = a.[RowPosition]
 AND r.[ColumnPosition] = a.[ColumnPosition];

IF (SELECT COUNT_BIG(*) FROM #HeaderMap) <> 1856
   OR (SELECT COUNT_BIG(DISTINCT [AuditHeaderId]) FROM #HeaderMap) <> 1856
   OR (SELECT COUNT_BIG(DISTINCT [RegisterUzTemplateHeaderId]) FROM #HeaderMap) <> 1856
   OR (SELECT COUNT_BIG(*) FROM [Template].[Headers]) <> 1856
   OR (SELECT COUNT_BIG(*) FROM [RegisterUZ].[Templates].[TemplateHeader]) <> 1856
    THROW 51204, 'Expected a unique 1,856-row header-coordinate mapping.', 1;

IF EXISTS
(
    SELECT 1
    FROM #HeaderMap hm
    JOIN [RegisterUZ].[Templates].[TemplateHeader] r
      ON r.[TemplateHeaderId] = hm.[RegisterUzTemplateHeaderId]
    WHERE r.[RowPosition] IS NULL
       OR r.[ColumnPosition] IS NULL
       OR r.[RowSpan] IS NULL
       OR r.[ColumnSpan] IS NULL
)
    THROW 51212, 'A canonical header contains NULL layout data that AuditAddIn requires.', 1;

/* Establish unique RowNumber keys for ordinary rows. */
SELECT r.[TableId] AS [AuditTableId], r.[RowNumber],
       COUNT_BIG(*) AS [Occurrences], MIN(r.[Id]) AS [AuditRowId]
INTO #AuditRowKeys
FROM [Template].[Rows] r
GROUP BY r.[TableId], r.[RowNumber];

SELECT r.[TemplateTableId] AS [RegisterUzTemplateTableId], r.[RowNumber],
       COUNT_BIG(*) AS [Occurrences], MIN(r.[TemplateRowId]) AS [RegisterUzTemplateRowId]
INTO #RegisterUzRowKeys
FROM [RegisterUZ].[Templates].[TemplateRow] r
GROUP BY r.[TemplateTableId], r.[RowNumber];

SELECT
    a.[AuditRowId], tm.[AuditTableId],
    r.[RegisterUzTemplateRowId], rr.[RowOrdinal]
INTO #OrdinaryRowMap
FROM #TableMap tm
JOIN #AuditRowKeys a
  ON a.[AuditTableId] = tm.[AuditTableId]
 AND a.[Occurrences] = 1
JOIN #RegisterUzRowKeys r
  ON r.[RegisterUzTemplateTableId] = tm.[RegisterUzTemplateTableId]
 AND r.[RowNumber] = a.[RowNumber]
 AND r.[Occurrences] = 1
JOIN [RegisterUZ].[Templates].[TemplateRow] rr
  ON rr.[TemplateRowId] = r.[RegisterUzTemplateRowId];

IF (SELECT COUNT_BIG(*) FROM #OrdinaryRowMap) <> 5922
   OR (SELECT COUNT_BIG(DISTINCT [AuditRowId]) FROM #OrdinaryRowMap) <> 5922
   OR (SELECT COUNT_BIG(DISTINCT [RegisterUzTemplateRowId]) FROM #OrdinaryRowMap) <> 5922
    THROW 51205, 'Expected a unique 5,922-row RowNumber mapping.', 1;

/* Identify the exact 24 corrupted legacy projections: source NULL became
   local zero, counts agree, and every row in each table belongs to the group. */
SELECT
    tm.[AuditTableId], tm.[TableErpId], tm.[RegisterUzTemplateId],
    tm.[TableOrdinal], tm.[RegisterUzTemplateTableId],
    a.[AuditRowCount], r.[RegisterUzRowCount]
INTO #AffectedTables
FROM #TableMap tm
JOIN
(
    SELECT [TableId], COUNT_BIG(*) AS [AuditRowCount]
    FROM [Template].[Rows]
    GROUP BY [TableId]
    HAVING SUM(CASE WHEN [RowNumber] = 0 THEN 1 ELSE 0 END) = COUNT_BIG(*)
) a ON a.[TableId] = tm.[AuditTableId]
JOIN
(
    SELECT [TemplateTableId], COUNT_BIG(*) AS [RegisterUzRowCount]
    FROM [RegisterUZ].[Templates].[TemplateRow]
    GROUP BY [TemplateTableId]
    HAVING SUM(CASE WHEN [RowNumber] IS NULL THEN 1 ELSE 0 END) = COUNT_BIG(*)
) r ON r.[TemplateTableId] = tm.[RegisterUzTemplateTableId]
WHERE a.[AuditRowCount] = r.[RegisterUzRowCount];

IF (SELECT COUNT_BIG(*) FROM #AffectedTables) <> 24
   OR (SELECT SUM([AuditRowCount]) FROM #AffectedTables) <> 580
   OR (SELECT SUM([RegisterUzRowCount]) FROM #AffectedTables) <> 580
    THROW 51206, 'Expected exactly 24 rebuild tables and 580 rows on each side.', 1;

/* Reconfirm that rebuilding cannot discard local row enrichment. */
IF EXISTS
(
    SELECT 1
    FROM #AffectedTables a
    JOIN [Template].[Rows] r ON r.[TableId] = a.[AuditTableId]
    WHERE NULLIF(LTRIM(RTRIM(r.[TextEn])), N'') IS NOT NULL
       OR ISNULL(r.[IsSumRow], 0) <> 0
       OR NULLIF(LTRIM(RTRIM(r.[CategorySk])), N'') IS NOT NULL
       OR NULLIF(LTRIM(RTRIM(r.[MappingCaptionSk])), N'') IS NOT NULL
)
    THROW 51207, 'A rebuild row contains local enrichment.', 1;

CREATE TABLE #BlockingDependencies
(
    [Dependency] nvarchar(100) NOT NULL,
    [ReferenceCount] bigint NOT NULL
);

INSERT #BlockingDependencies
SELECT N'Accounts.AcountGroupsUsage', COUNT_BIG(*)
FROM [Accounts].[AcountGroupsUsage] d
JOIN #AffectedTables a ON a.[AuditTableId] = d.[TableId]
HAVING COUNT_BIG(*) > 0
UNION ALL
SELECT N'Template.SumRows', COUNT_BIG(*)
FROM [Template].[SumRows] d
JOIN #AffectedTables a ON a.[AuditTableId] = d.[TableId]
HAVING COUNT_BIG(*) > 0
UNION ALL
SELECT N'Template.SumOrder', COUNT_BIG(*)
FROM [Template].[SumOrder] d
JOIN #AffectedTables a ON a.[AuditTableId] = d.[TableId]
HAVING COUNT_BIG(*) > 0
UNION ALL
SELECT N'Template.SumCalculationPlan.SumTable', COUNT_BIG(*)
FROM [Template].[SumCalculationPlan] d
JOIN #AffectedTables a ON a.[AuditTableId] = d.[SumTableId]
HAVING COUNT_BIG(*) > 0
UNION ALL
SELECT N'Template.SumCalculationPlan.SourceTable', COUNT_BIG(*)
FROM [Template].[SumCalculationPlan] d
JOIN #AffectedTables a ON a.[AuditTableId] = d.[SourceTableId]
HAVING COUNT_BIG(*) > 0;

IF EXISTS (SELECT 1 FROM #BlockingDependencies)
    THROW 51208, 'A rebuild table has local calculation or mapping dependencies.', 1;

IF EXISTS
(
    SELECT 1 FROM sys.foreign_keys
    WHERE [referenced_object_id] = OBJECT_ID(N'[Template].[Rows]')
)
    THROW 51209, 'A foreign key references Template.Rows.', 1;

/* Every AuditAddIn row must be either retained or rebuilt, exactly once. */
IF (SELECT COUNT_BIG(*) FROM #OrdinaryRowMap)
   + (SELECT SUM([AuditRowCount]) FROM #AffectedTables) <> 6502
    THROW 51210, 'The retained and rebuilt row populations do not total 6,502.', 1;

IF (SELECT COUNT_BIG(*) FROM [Template].[Rows]) <> 6502
   OR (SELECT COUNT_BIG(*) FROM [RegisterUZ].[Templates].[TemplateRow]) <> 6502
    THROW 51213, 'Expected exactly 6,502 rows in both projections.', 1;

/* New templates may be inserted automatically only when they have no tables. */
IF EXISTS
(
    SELECT 1
    FROM [RegisterUZ].[Templates].[FinancialReportTemplate] r
    LEFT JOIN [Template].[Templates] a ON a.[ErpId] = r.[RegisterUzTemplateId]
    JOIN [RegisterUZ].[Templates].[TemplateTable] rt
      ON rt.[RegisterUzTemplateId] = r.[RegisterUzTemplateId]
    WHERE a.[Id] IS NULL
)
    THROW 51211, 'A missing template has child tables and requires administrator-assigned TableErpId values.', 1;

/* Preview. */
SELECT
    (SELECT COUNT_BIG(*)
     FROM [RegisterUZ].[Templates].[FinancialReportTemplate] r
     LEFT JOIN [Template].[Templates] a ON a.[ErpId] = r.[RegisterUzTemplateId]
     WHERE a.[Id] IS NULL) AS [TemplatesToInsert],
    (SELECT COUNT_BIG(*) FROM #TableMap) AS [TablesToStamp],
    (SELECT COUNT_BIG(*) FROM #HeaderMap) AS [HeadersToStamp],
    (SELECT COUNT_BIG(*) FROM #OrdinaryRowMap) AS [RowsToRetainAndStamp],
    (SELECT COUNT_BIG(*) FROM #AffectedTables) AS [TablesToRebuild],
    (SELECT SUM([AuditRowCount]) FROM #AffectedTables) AS [RowsToRebuild];

SELECT
    [RegisterUzTemplateId], [TableOrdinal], [AuditTableId], [TableErpId],
    [AuditRowCount]
FROM #AffectedTables
ORDER BY [RegisterUzTemplateId], [TableOrdinal];

IF @ApplyChanges = 0
BEGIN
    PRINT 'PREVIEW ONLY. No persistent changes were made.';
    RETURN;
END;

BEGIN TRY
    BEGIN TRANSACTION;

    /* Dynamic batches are required because SQL Server compiles column
       references before columns added earlier in the outer batch exist. */
    EXEC(N'ALTER TABLE [Template].[Tables] ADD [TableOrdinal] int NULL;');
    EXEC(N'ALTER TABLE [Template].[Headers] ADD [HeaderOrdinal] int NULL;');
    EXEC(N'ALTER TABLE [Template].[Rows] ADD [RowOrdinal] int NULL;');

    EXEC(N'
        UPDATE t SET [TableOrdinal] = m.[TableOrdinal]
        FROM [Template].[Tables] t
        JOIN #TableMap m ON m.[AuditTableId] = t.[Id];

        UPDATE h SET [HeaderOrdinal] = m.[HeaderOrdinal]
        FROM [Template].[Headers] h
        JOIN #HeaderMap m ON m.[AuditHeaderId] = h.[Id];

        UPDATE r SET [RowOrdinal] = m.[RowOrdinal]
        FROM [Template].[Rows] r
        JOIN #OrdinaryRowMap m ON m.[AuditRowId] = r.[Id];
    ');

    /* Only rows proven unreferenced and unenriched are replaced. */
    DELETE r
    FROM [Template].[Rows] r
    JOIN #AffectedTables a ON a.[AuditTableId] = r.[TableId];

    EXEC(N'
        INSERT [Template].[Rows]
        (
            [TableId], [TableErpId], [RowNumber], [Designation],
            [TextSk], [TextEn], [IsSumRow], [CategorySk],
            [MappingCaptionSk], [RowOrdinal]
        )
        SELECT
            a.[AuditTableId], CONVERT(int, a.[TableErpId]), r.[RowNumber],
            r.[Designation], r.[TextSk], NULL, CONVERT(tinyint, 0), NULL, NULL,
            r.[RowOrdinal]
        FROM #AffectedTables a
        JOIN [RegisterUZ].[Templates].[TemplateRow] r
          ON r.[TemplateTableId] = a.[RegisterUzTemplateTableId];

        ALTER TABLE [Template].[Tables] ALTER COLUMN [TableOrdinal] int NOT NULL;
        ALTER TABLE [Template].[Headers] ALTER COLUMN [HeaderOrdinal] int NOT NULL;
        ALTER TABLE [Template].[Rows] ALTER COLUMN [RowOrdinal] int NOT NULL;

        CREATE UNIQUE INDEX [UQ_Template_Tables_TemplateErpId_TableOrdinal]
            ON [Template].[Tables] ([TemplateErpId], [TableOrdinal]);
        CREATE UNIQUE INDEX [UQ_Template_Headers_TableId_HeaderOrdinal]
            ON [Template].[Headers] ([TableId], [HeaderOrdinal]);
        CREATE UNIQUE INDEX [UQ_Template_Rows_TableId_RowOrdinal]
            ON [Template].[Rows] ([TableId], [RowOrdinal]);
    ');

    /* Refresh official fields while preserving all local enrichment columns. */
    UPDATE a
    SET [Name] = r.[Name],
        [MfSpecification] = r.[MinistrySpecification],
        [ValidFrom] = r.[ValidFrom],
        [ValidTo] = r.[ValidTo]
    FROM [Template].[Templates] a
    JOIN [RegisterUZ].[Templates].[FinancialReportTemplate] r
      ON r.[RegisterUzTemplateId] = a.[ErpId];

    INSERT [Template].[Templates]
        ([ErpId], [Name], [MfSpecification], [ValidFrom], [ValidTo])
    SELECT CONVERT(int, r.[RegisterUzTemplateId]), r.[Name],
           r.[MinistrySpecification], r.[ValidFrom], r.[ValidTo]
    FROM [RegisterUZ].[Templates].[FinancialReportTemplate] r
    WHERE NOT EXISTS
    (
        SELECT 1 FROM [Template].[Templates] a
        WHERE a.[ErpId] = r.[RegisterUzTemplateId]
    );

    UPDATE a
    SET [NameSk] = r.[NameSk],
        [NumberOfColumns] = r.[NumberOfColumns],
        [NumberOfDataColumns] = r.[NumberOfDataColumns]
    FROM [Template].[Tables] a
    JOIN #TableMap tm ON tm.[AuditTableId] = a.[Id]
    JOIN [RegisterUZ].[Templates].[TemplateTable] r
      ON r.[TemplateTableId] = tm.[RegisterUzTemplateTableId];

    UPDATE a
    SET [TextSk] = r.[TextSk],
        [RowPosition] = r.[RowPosition],
        [ColumnPosition] = r.[ColumnPosition],
        [RowSpan] = r.[RowSpan],
        [ColumnSpan] = r.[ColumnSpan]
    FROM [Template].[Headers] a
    JOIN #HeaderMap hm ON hm.[AuditHeaderId] = a.[Id]
    JOIN [RegisterUZ].[Templates].[TemplateHeader] r
      ON r.[TemplateHeaderId] = hm.[RegisterUzTemplateHeaderId];

    UPDATE a
    SET [RowNumber] = r.[RowNumber],
        [Designation] = r.[Designation],
        [TextSk] = r.[TextSk]
    FROM [Template].[Rows] a
    JOIN #OrdinaryRowMap rm ON rm.[AuditRowId] = a.[Id]
    JOIN [RegisterUZ].[Templates].[TemplateRow] r
      ON r.[TemplateRowId] = rm.[RegisterUzTemplateRowId];

    COMMIT TRANSACTION;
    PRINT 'AuditAddIn RegisterUZ template synchronization V2 completed.';
END TRY
BEGIN CATCH
    IF XACT_STATE() <> 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
