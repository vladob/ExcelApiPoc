# RegisterUZ single-IČO loader

Configure the database connection without committing credentials:

```powershell
dotnet user-secrets --project .\RegisterUz.Loader set `
  "ConnectionStrings:RegisterUZ" `
  "Server=SRVHPV;Database=RegisterUZ;User ID=...;Password=...;TrustServerCertificate=True"
```

Then run:

```powershell
dotnet run --project .\RegisterUz.Loader -- 00325554
```

The loader retrieves one accounting entity and follows the child identifiers
declared by its details. It validates all parent links and structured table
dimensions before saving the complete package in one SQL transaction.

No attachment binaries are downloaded. Attachment metadata is persisted.

## Change-feed collection

Apply `SQL code/RegisterUZ_ChangeFeedV2_2.sql` once before the first change-feed
run. The collector tracks four independent feeds: accounting entities,
financial statements, financial reports, and annual reports.

The first timestamp initializes only feeds that do not yet have a checkpoint.
Later invocations resume or advance each durable checkpoint:

```powershell
dotnet run --project .\RegisterUz.Loader -- `
  changes 2026-08-30T18:00:00Z 100 1
```

The optional final arguments are page size (1-10000, default 100) and maximum
pages per feed (default 1). A bounded run pauses after that many pages and the
next invocation resumes from the stored `pokracovat-za-id` value. Only the
final page advances the inclusive `zmenene-od` timestamp.

## Change processing

Apply `SQL code/RegisterUZ_ChangeProcessingV1.sql` after the change-feed
migration. It adds a change-only counter, an observation-resolution work
table, and a deduplicating accounting-entity refresh queue.

Run a deliberately bounded worker pass:

```powershell
dotnet run --project .\RegisterUz.Loader -- process-changes 25 1
```

The optional arguments are the maximum observations to resolve (default 25)
and maximum accounting entities to refresh (default 1). Either can be zero to
run only the other phase. Claims use 15-minute expiring leases.

Accounting-entity observations resolve directly. Financial statements and
annual reports expose their entity ID. A financial report is resolved through
exactly one statement or annual-report parent. Resolved observations enqueue
an entity generation; multiple changed children therefore collapse into one
complete, idempotent entity-package refresh.

Every entity-refresh batch retrieves the seven shared catalogs once. The same
immutable snapshot is used for template resolution in every claimed entity;
the first successful entity run synchronizes it to SQL. If that entity fails,
the next entity safely retries catalog synchronization without repeating the
seven HTTP requests.
