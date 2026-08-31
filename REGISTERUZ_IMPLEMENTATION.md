# RegisterUZ single-IČO vertical slice

## Projects

- `RegisterUz.Core` — API/domain contracts and package interfaces.
- `RegisterUz.Client` — typed client for the official anonymous API.
- `RegisterUz.Persistence.SqlServer` — transactional persistence into `RegisterUZ`.
- `RegisterUz.Sync` — graph traversal and parent validation.
- `RegisterUz.Loader` — console entry point for the first operational test.
- `RegisterUz.Tests` — API deserialization, ordering, and canonical-hash tests.

## Raw payload identity

`Raw.PayloadVersion` retains two SHA-256 values:

- `PayloadSha256` verifies the exact UTF-8 response stored in that row.
- `CanonicalSha256` identifies the semantic payload version and is the
  idempotence key.

Canonicalization ignores JSON object-property order, insignificant whitespace,
and order changes in the relationship-ID arrays `idUctovnychZavierok`,
`idVyrocnychSprav`, and `idUctovnychVykazov`. It deliberately preserves every
other array order, including financial-report tables, rows, values, headers,
and attachments.

Use `RegisterUZinitializationV2_1.sql` for a new database or rerun it against
an existing V2 database. V2.1 adds catalog synchronization without discarding
existing entity and report data. It remains incompatible with V1.

## Database schemas

- `Reference` contains general classifications such as legal forms, locations,
  organization sizes, ownership types, and SK NACE codes.
- `Templates` contains the complete financial-report-template aggregate:
  `FinancialReportTemplate`, `TemplateTable`, `TemplateHeader`, and
  `TemplateRow`.

## Shared catalogs

Every top-level synchronization run retrieves these seven small official
catalogs once: templates, legal forms, SK NACE, ownership types, organization
sizes, regions, and districts. The `sidla` catalog is deliberately excluded;
its source code remains on the accounting entity without a classification FK.

- `Sync.CatalogObservation` stores the compressed exact response, exact hash,
  order-independent catalog hash, record count, and change flag.
- `Sync.CatalogItemState` stores the current canonical hash and presence of
  every official catalog item.
- `Sync.CatalogChange` records inserted, updated, removed, and reappearing
  items and supports administrative review and later AuditAddIn publication.
- `Sync.Run` contains separate catalog summary counts, so entity-graph counts
  remain directly comparable between runs.

The first complete catalog load establishes a baseline. It records inserted
items but does not create pending-review warnings. Later changes do.

## First test

Configure `ConnectionStrings:RegisterUZ` as described in
`RegisterUz.Loader/README.md`, rebuild, and run:

```powershell
dotnet run --project .\RegisterUz.Loader -- 00325554
```

## Verification queries

```sql
USE [RegisterUZ];

SELECT *
FROM [Registry].[AccountingEntity]
WHERE [Ico] = '00325554';

SELECT fs.*
FROM [Reporting].[FinancialStatement] fs
JOIN [Registry].[AccountingEntity] ae
  ON ae.[RegisterUzEntityId] = fs.[RegisterUzEntityId]
WHERE ae.[Ico] = '00325554'
ORDER BY fs.[PeriodTo], fs.[RegisterUzStatementId];

SELECT
    fr.[RegisterUzFinancialReportId],
    fr.[RegisterUzStatementId],
    fr.[RegisterUzAnnualReportId],
    fr.[RegisterUzTemplateId],
    rt.[TableOrdinal],
    tt.[TableOrdinal] AS [TemplateTableOrdinal],
    COUNT_BIG(rv.[ValueOrdinal]) AS [ValueCount],
    MAX(rv.[RowOrdinal]) + 1 AS [RowCount],
    MAX(rv.[DataColumnOrdinal]) + 1 AS [DataColumnCount]
FROM [Reporting].[FinancialReport] fr
LEFT JOIN [Reporting].[FinancialReportTable] rt
  ON rt.[RegisterUzFinancialReportId] = fr.[RegisterUzFinancialReportId]
LEFT JOIN [Templates].[TemplateTable] tt
  ON tt.[TemplateTableId] = rt.[TemplateTableId]
LEFT JOIN [Reporting].[FinancialReportValue] rv
  ON rv.[FinancialReportTableId] = rt.[FinancialReportTableId]
GROUP BY
    fr.[RegisterUzFinancialReportId],
    fr.[RegisterUzStatementId],
    fr.[RegisterUzAnnualReportId],
    fr.[RegisterUzTemplateId],
    rt.[TableOrdinal],
    tt.[TableOrdinal]
ORDER BY
    fr.[RegisterUzFinancialReportId],
    rt.[TableOrdinal];

SELECT * FROM [Sync].[Run] ORDER BY [SyncRunId] DESC;
SELECT * FROM [Sync].[CatalogObservation] ORDER BY [CatalogObservationId] DESC;
SELECT * FROM [Sync].[CatalogChange] ORDER BY [CatalogChangeId] DESC;
SELECT * FROM [Sync].[Error] ORDER BY [SyncErrorId] DESC;
```

## Intentional scope boundary

This first slice downloads attachment metadata but not attachment binaries.
It loads a complete valid package transactionally. Capturing unsuccessful HTTP
or invalid-JSON responses before deserialization belongs to the later general
change-feed worker, where every HTTP request will have its own `Sync.Request`
record.

## Operational change feeds

`RegisterUz.Loader changes` collects the four independent official identifier
feeds into `Sync.ObservedObject`. Each object type has its own exact-UTC
checkpoint. Page persistence is atomic with the request audit and continuation
cursor, and bounded runs use the `Paused` state so they can resume safely.

The official API has no upper-bound timestamp. At the start of a new scan the
collector records `ScanStartedAtUtc`; after its final page, that value becomes
the next inclusive `ChangedSinceUtc`. This deliberate overlap prevents changes
made during a long scan from being skipped.

## Change processing

Stage 2 separates observation resolution from entity refresh:

1. `Sync.ObservedObject.ChangeObservationCount` is incremented only by an
   official change-feed page. Package-detail observations cannot create a
   processing feedback loop.
2. `Sync.ObservedObjectWork` claims unseen change generations with expiring
   leases and resolves every object to its accounting entity.
3. `Sync.EntityRefreshQueue` groups any number of changed children by entity.
   Its requested/completed generation counters preserve a change that arrives
   while a refresh is running.
4. The refresh worker loads the complete entity graph by RegisterUZ entity ID
   and acknowledges only the generation covered by that successful load.

Both phases are bounded independently by `RegisterUz.Loader process-changes`.
Individual failures are retained for retry and do not prevent unrelated work
items from completing.

One `process-changes` entity batch retrieves the seven shared catalogs once.
All claimed entities use that same snapshot, and only the first successful
entity run persists its catalog observation. This avoids seven repeated HTTP
requests and repeated catalog comparisons for every additional entity in the
batch while retaining the existing per-entity transaction and failure model.
