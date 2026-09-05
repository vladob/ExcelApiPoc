# RegisterUZ database

`RegisterUZ` is an independently reusable SQL Server mirror of data retrieved
from the Slovak Register of Financial Statements. ExcelApiPoc currently reads
it and performs controlled on-demand loading, but the database is not owned by
the Excel add-in and may serve other solutions.

Audit-specific frameworks, calculation rules, report mappings and auditor
configuration belong in `AuditAddIn`, not in `RegisterUZ`.

## Creation and upgrade sequence

For a new database, apply:

1. `Baseline/V2_1/000_CreateRegisterUzDatabase.sql`
2. `Migrations/ChangeFeedV2_2/010_UpgradeChangeFeedCheckpoint.sql`
3. `Migrations/ChangeProcessingV1/010_AddChangeProcessing.sql`
4. `Migrations/SparseFinancialReportValuesV1/010_ConvertToSparseFinancialReportValues.sql`

Review every script before execution. Some migration scripts start in preview
mode and require an explicit `@ApplyChanges = 1` only after their result sets
have been checked.

The V2.2 change-feed migration replaces the original checkpoint representation.
It refuses to discard a populated checkpoint; existing state must be migrated
deliberately.

The sparse-value migration removes blank financial-report cells while retaining
explicit zeroes. Its canonical raw JSON remains unchanged.

## Directory conventions

- `Baseline` creates a named starting version.
- `Migrations` contains reusable upgrades needed to reach later structures.
- `DataUpdates` is reserved for controlled reference/catalog data publication.
- `Maintenance/Applied` records environment-specific repairs already performed.
- `Validation` is reserved for reusable, non-mutating verification.

Scripts under `Maintenance/Applied` are historical evidence and must not be
executed automatically against a new or unrelated RegisterUZ database.

## Application access

On the current single-server deployment, ExcelApiPoc.Api connects through the
Windows identity `IIS APPPOOL\ExcelApiPoc.Api`. RegisterUZ requires both read
access and the narrowly scoped write permissions used by on-demand loading and
synchronization. No personal administrator SQL login or plaintext SQL password
should be used by the API.

If the API or SQL Server moves to another machine, replace the local application
pool identity with an appropriately scoped domain or group-managed service
account.

## Consumer boundary

Consumers should depend on stable RegisterUZ database/API contracts rather than
the needs of one workbook. Cross-database publication into `AuditAddIn` must be
an explicit, reviewed administrative operation.
