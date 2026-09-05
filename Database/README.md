# Database scripts

This directory contains the reproducible database definitions and controlled
database changes used by ExcelApiPoc.

## Databases

- `AuditAddIn` belongs exclusively to the ExcelApiPoc solution. Its baseline,
  migrations, calculation metadata and validation scripts are maintained here.
- `RegisterUZ` is an independent mirror of the Slovak Register of Financial
  Statements. ExcelApiPoc is one consumer, but the database and its lifecycle
  are intentionally reusable by other solutions.

## Directory conventions

- `Baseline` creates a database at a named starting version.
- `Migrations` upgrades an existing database structure or representation.
- `DataUpdates` contains reviewed domain/configuration additions that are not
  general schema migrations.
- `Maintenance` records controlled operational repairs.
- `Validation` contains reusable, non-mutating verification scripts.

Baseline and migration scripts must be committed before they are applied to a
shared environment. Do not store passwords, API keys, backup files or
environment-specific secrets in this directory.

Each database README defines its execution order and any scripts that must not
be rerun.
