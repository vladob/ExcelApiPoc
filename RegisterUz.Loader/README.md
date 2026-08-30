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

This stage records identifiers only. Detail loading from pending observations
is intentionally a separate worker stage.
