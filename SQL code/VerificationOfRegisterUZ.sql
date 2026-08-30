SELECT
    [SyncRunId],
    [Status],
    [ObservedIdCount],
    [DetailRequestCount],
    [InsertedObjectCount],
    [UpdatedObjectCount],
    [UnchangedObjectCount],
    [CatalogObservationCount],
    [CatalogInsertedCount],
    [CatalogUpdatedCount],
    [CatalogRemovedCount],
    [CatalogReviewRequiredCount],
    [ErrorCount]
FROM [RegisterUZ].[Sync].[Run]
WHERE [SyncRunId] IN (5, 6)
ORDER BY [SyncRunId];

/*
SyncRunId	Status	ObservedIdCount	DetailRequestCount	InsertedObjectCount	UpdatedObjectCount	UnchangedObjectCount	CatalogObservationCount	CatalogInsertedCount	CatalogUpdatedCount	CatalogRemovedCount	CatalogReviewRequiredCount	ErrorCount
5	Completed	89	89	0	0	89	7	1131	0	0	0	0
6	Completed	89	89	0	0	89	7	0	0	0	0	0
*/

SELECT
    [CatalogCode],
    [RecordCount],
    [HasChanged]
FROM [RegisterUZ].[Sync].[CatalogObservation]
WHERE [SyncRunId] = 6
ORDER BY [CatalogCode];

/*
CatalogCode	RecordCount	HasChanged
Districts	81	0
LegalForms	116	0
OrganizationSizes	23	0
OwnershipTypes	10	0
Regions	10	0
SkNace	646	0
Templates	245	0
*/

SELECT '[Templates].[FinancialReportTemplate]' AS [Table], COUNT_BIG(*) AS [Records] FROM [Templates].[FinancialReportTemplate] -- 245
UNION ALL SELECT '[Templates].[TemplateTable]' AS [Table], COUNT_BIG(*) AS [Records] FROM [Templates].[TemplateTable]           -- 167
UNION ALL SELECT '[Templates].[TemplateHeader]' AS [Table], COUNT_BIG(*) AS [Records] FROM [Templates].[TemplateHeader]          -- 1856
UNION ALL SELECT '[Templates].[TemplateRow]' AS [Table], COUNT_BIG(*) AS [Records] FROM [Templates].[TemplateRow]             -- 6502

UNION ALL SELECT '[Reference].[LegalForm]' AS [Table], COUNT_BIG(*) AS [Records] FROM [Reference].[LegalForm]                -- 116
UNION ALL SELECT '[Reference].[SkNace]' AS [Table], COUNT_BIG(*) AS [Records] FROM [Reference].[SkNace]                   -- 646
UNION ALL SELECT '[Reference].[OwnershipType]' AS [Table], COUNT_BIG(*) AS [Records] FROM [Reference].[OwnershipType]            -- 10
UNION ALL SELECT '[Reference].[OrganizationSize]' AS [Table], COUNT_BIG(*) AS [Records] FROM [Reference].[OrganizationSize]         -- 23
UNION ALL SELECT '[Reference].[Location]' AS [Table], COUNT_BIG(*) AS [Records] FROM [Reference].[Location];                 -- 91

/*

Table	Records
[Templates].[FinancialReportTemplate]	245
[Templates].[TemplateTable]	167
[Templates].[TemplateHeader]	1856
[Templates].[TemplateRow]	6502
[Reference].[LegalForm]	116
[Reference].[SkNace]	646
[Reference].[OwnershipType]	10
[Reference].[OrganizationSize]	23
[Reference].[Location]	90
*/

SELECT
    COUNT_BIG(*) AS [BaselineChanges],
    SUM(CASE WHEN [RequiresReview] = 1 THEN 1 ELSE 0 END) AS [PendingReview]
FROM [RegisterUZ].[Sync].[CatalogChange];

/*
1131	0
*/