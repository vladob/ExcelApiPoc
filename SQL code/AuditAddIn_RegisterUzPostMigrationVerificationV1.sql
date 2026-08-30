/*
    Read-only verification after
    AuditAddIn_RegisterUzTemplateSynchronizationV2.sql.

    Expected result:
      - inventory counts agree;
      - every Missing/Differing count is zero;
      - every ordinal is non-NULL and unique;
      - all three unique ordinal indexes exist.
*/

USE [AuditAddIn];
GO

SET NOCOUNT OFF;
SET XACT_ABORT ON;

IF DB_ID(N'RegisterUZ') IS NULL
    THROW 51301, 'Database RegisterUZ was not found.', 1;

IF COL_LENGTH(N'Template.Tables', N'TableOrdinal') IS NULL
   OR COL_LENGTH(N'Template.Headers', N'HeaderOrdinal') IS NULL
   OR COL_LENGTH(N'Template.Rows', N'RowOrdinal') IS NULL
    THROW 51302, 'One or more persistent ordinal columns are missing.', 1;

DROP TABLE IF EXISTS #TableMap;

SELECT
    a.[Id] AS [AuditTableId],
    r.[TemplateTableId] AS [RegisterUzTemplateTableId],
    COALESCE(CONVERT(bigint, a.[TemplateErpId]), r.[RegisterUzTemplateId])
        AS [RegisterUzTemplateId],
    COALESCE(a.[TableOrdinal], r.[TableOrdinal]) AS [TableOrdinal]
INTO #TableMap
FROM [Template].[Tables] a
FULL OUTER JOIN [RegisterUZ].[Templates].[TemplateTable] r
  ON r.[RegisterUzTemplateId] = a.[TemplateErpId]
 AND r.[TableOrdinal] = a.[TableOrdinal];

/* 1. Inventory. */
SELECT N'Templates' AS [ObjectType],
       (SELECT COUNT_BIG(*) FROM [Template].[Templates]) AS [AuditAddInCount],
       (SELECT COUNT_BIG(*) FROM [RegisterUZ].[Templates].[FinancialReportTemplate]) AS [RegisterUzCount]
UNION ALL
SELECT N'Tables',
       (SELECT COUNT_BIG(*) FROM [Template].[Tables]),
       (SELECT COUNT_BIG(*) FROM [RegisterUZ].[Templates].[TemplateTable])
UNION ALL
SELECT N'Headers',
       (SELECT COUNT_BIG(*) FROM [Template].[Headers]),
       (SELECT COUNT_BIG(*) FROM [RegisterUZ].[Templates].[TemplateHeader])
UNION ALL
SELECT N'Rows',
       (SELECT COUNT_BIG(*) FROM [Template].[Rows]),
       (SELECT COUNT_BIG(*) FROM [RegisterUZ].[Templates].[TemplateRow]);

/* 2. Missing records and official-field differences. */
SELECT
    (SELECT COUNT_BIG(*)
     FROM [Template].[Templates] a
     FULL OUTER JOIN [RegisterUZ].[Templates].[FinancialReportTemplate] r
       ON r.[RegisterUzTemplateId] = a.[ErpId]
     WHERE a.[Id] IS NULL OR r.[RegisterUzTemplateId] IS NULL) AS [TemplatesMissing],
    (SELECT COUNT_BIG(*)
     FROM [Template].[Templates] a
     JOIN [RegisterUZ].[Templates].[FinancialReportTemplate] r
       ON r.[RegisterUzTemplateId] = a.[ErpId]
     WHERE ISNULL(a.[Name], N'') <> ISNULL(r.[Name], N'')
        OR ISNULL(a.[MfSpecification], N'') <> ISNULL(r.[MinistrySpecification], N'')
        OR ISNULL(a.[ValidFrom], CONVERT(date, '00010101', 112))
           <> ISNULL(r.[ValidFrom], CONVERT(date, '00010101', 112))
        OR ISNULL(a.[ValidTo], CONVERT(date, '99991231', 112))
           <> ISNULL(r.[ValidTo], CONVERT(date, '99991231', 112))) AS [TemplateOfficialFieldsDiffer],
    (SELECT COUNT_BIG(*) FROM #TableMap
     WHERE [AuditTableId] IS NULL OR [RegisterUzTemplateTableId] IS NULL) AS [TablesMissing],
    (SELECT COUNT_BIG(*)
     FROM #TableMap m
     JOIN [Template].[Tables] a ON a.[Id] = m.[AuditTableId]
     JOIN [RegisterUZ].[Templates].[TemplateTable] r
       ON r.[TemplateTableId] = m.[RegisterUzTemplateTableId]
     WHERE ISNULL(a.[NameSk], N'') <> ISNULL(r.[NameSk], N'')
        OR ISNULL(a.[NumberOfColumns], -1) <> ISNULL(r.[NumberOfColumns], -1)
        OR ISNULL(a.[NumberOfDataColumns], -1) <> ISNULL(r.[NumberOfDataColumns], -1))
        AS [TableOfficialFieldsDiffer],
    (SELECT COUNT_BIG(*)
     FROM #TableMap m
     FULL OUTER JOIN [Template].[Headers] a
       ON a.[TableId] = m.[AuditTableId]
     FULL OUTER JOIN [RegisterUZ].[Templates].[TemplateHeader] r
       ON r.[TemplateTableId] = m.[RegisterUzTemplateTableId]
      AND r.[HeaderOrdinal] = a.[HeaderOrdinal]
     WHERE a.[Id] IS NULL OR r.[TemplateHeaderId] IS NULL) AS [HeadersMissing],
    (SELECT COUNT_BIG(*)
     FROM #TableMap m
     JOIN [Template].[Headers] a ON a.[TableId] = m.[AuditTableId]
     JOIN [RegisterUZ].[Templates].[TemplateHeader] r
       ON r.[TemplateTableId] = m.[RegisterUzTemplateTableId]
      AND r.[HeaderOrdinal] = a.[HeaderOrdinal]
     WHERE ISNULL(a.[TextSk], N'') <> ISNULL(r.[TextSk], N'')
        OR ISNULL(a.[RowPosition], -1) <> ISNULL(r.[RowPosition], -1)
        OR ISNULL(a.[ColumnPosition], -1) <> ISNULL(r.[ColumnPosition], -1)
        OR ISNULL(a.[RowSpan], -1) <> ISNULL(r.[RowSpan], -1)
        OR ISNULL(a.[ColumnSpan], -1) <> ISNULL(r.[ColumnSpan], -1))
        AS [HeaderOfficialFieldsDiffer],
    (SELECT COUNT_BIG(*)
     FROM #TableMap m
     FULL OUTER JOIN [Template].[Rows] a
       ON a.[TableId] = m.[AuditTableId]
     FULL OUTER JOIN [RegisterUZ].[Templates].[TemplateRow] r
       ON r.[TemplateTableId] = m.[RegisterUzTemplateTableId]
      AND r.[RowOrdinal] = a.[RowOrdinal]
     WHERE a.[Id] IS NULL OR r.[TemplateRowId] IS NULL) AS [RowsMissing],
    (SELECT COUNT_BIG(*)
     FROM #TableMap m
     JOIN [Template].[Rows] a ON a.[TableId] = m.[AuditTableId]
     JOIN [RegisterUZ].[Templates].[TemplateRow] r
       ON r.[TemplateTableId] = m.[RegisterUzTemplateTableId]
      AND r.[RowOrdinal] = a.[RowOrdinal]
     WHERE ISNULL(a.[RowNumber], -2147483648) <> ISNULL(r.[RowNumber], -2147483648)
        OR ISNULL(a.[Designation], N'') <> ISNULL(r.[Designation], N'')
        OR ISNULL(a.[TextSk], N'') <> ISNULL(r.[TextSk], N''))
        AS [RowOfficialFieldsDiffer];

/* 3. Persisted identity integrity. */
SELECT
    (SELECT COUNT_BIG(*) FROM [Template].[Tables] WHERE [TableOrdinal] IS NULL)
        AS [NullTableOrdinals],
    (SELECT COUNT_BIG(*) FROM [Template].[Headers] WHERE [HeaderOrdinal] IS NULL)
        AS [NullHeaderOrdinals],
    (SELECT COUNT_BIG(*) FROM [Template].[Rows] WHERE [RowOrdinal] IS NULL)
        AS [NullRowOrdinals],
    (SELECT COUNT_BIG(*) FROM
       (SELECT [TemplateErpId], [TableOrdinal] FROM [Template].[Tables]
        GROUP BY [TemplateErpId], [TableOrdinal] HAVING COUNT_BIG(*) > 1) d)
        AS [DuplicateTableOrdinals],
    (SELECT COUNT_BIG(*) FROM
       (SELECT [TableId], [HeaderOrdinal] FROM [Template].[Headers]
        GROUP BY [TableId], [HeaderOrdinal] HAVING COUNT_BIG(*) > 1) d)
        AS [DuplicateHeaderOrdinals],
    (SELECT COUNT_BIG(*) FROM
       (SELECT [TableId], [RowOrdinal] FROM [Template].[Rows]
        GROUP BY [TableId], [RowOrdinal] HAVING COUNT_BIG(*) > 1) d)
        AS [DuplicateRowOrdinals];

/* 4. Required unique indexes. */
SELECT v.[IndexName],
       CONVERT(bit, CASE WHEN i.[index_id] IS NULL THEN 0 ELSE 1 END) AS [Exists],
       ISNULL(i.[is_unique], 0) AS [IsUnique]
FROM
(
    VALUES
      (OBJECT_ID(N'[Template].[Tables]'), N'UQ_Template_Tables_TemplateErpId_TableOrdinal'),
      (OBJECT_ID(N'[Template].[Headers]'), N'UQ_Template_Headers_TableId_HeaderOrdinal'),
      (OBJECT_ID(N'[Template].[Rows]'), N'UQ_Template_Rows_TableId_RowOrdinal')
) v ([ObjectId], [IndexName])
LEFT JOIN sys.indexes i
  ON i.[object_id] = v.[ObjectId]
 AND i.[name] = v.[IndexName]
ORDER BY v.[IndexName];

/* 5. Local enrichment retained in AuditAddIn (informational counts). */
SELECT
    (SELECT COUNT_BIG(*) FROM [Template].[Tables]
     WHERE NULLIF(LTRIM(RTRIM([NameEn])), N'') IS NOT NULL) AS [TablesWithNameEn],
    (SELECT COUNT_BIG(*) FROM [Template].[Headers]
     WHERE NULLIF(LTRIM(RTRIM([TextEn])), N'') IS NOT NULL) AS [HeadersWithTextEn],
    (SELECT COUNT_BIG(*) FROM [Template].[Rows]
     WHERE NULLIF(LTRIM(RTRIM([TextEn])), N'') IS NOT NULL) AS [RowsWithTextEn],
    (SELECT COUNT_BIG(*) FROM [Template].[Rows]
     WHERE ISNULL([IsSumRow], 0) <> 0) AS [RowsMarkedAsSum],
    (SELECT COUNT_BIG(*) FROM [Template].[Rows]
     WHERE NULLIF(LTRIM(RTRIM([CategorySk])), N'') IS NOT NULL) AS [RowsWithCategorySk],
    (SELECT COUNT_BIG(*) FROM [Template].[Rows]
     WHERE NULLIF(LTRIM(RTRIM([MappingCaptionSk])), N'') IS NOT NULL)
        AS [RowsWithMappingCaptionSk];

PRINT 'Post-migration verification completed. All Missing/Differing/Null/Duplicate counts should be zero.';
