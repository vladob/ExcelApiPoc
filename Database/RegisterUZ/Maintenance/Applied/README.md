# Applied RegisterUZ maintenance

This directory records one-time corrections already applied to the original
RegisterUZ deployment.

These scripts are retained for traceability. They are not baseline scripts,
reusable migrations, or part of automatic deployment, and they must not be run
against a new or unrelated RegisterUZ database.

| Script | Historical purpose |
|---|---|
| `2026_08_RepairChangeProcessingStatuses.sql` | Corrected package-only observation work statuses created before the application handled observation counts correctly. |
| `2026_08_RelabelEntityRefreshRuns.sql` | Relabelled earlier queue-driven package runs as `EntityRefresh`. |
| `2026_08_CloseRun8TemplateCatalogChanges.sql` | Administratively closed the verified template-structure changes from synchronization run 8 after publication to AuditAddIn. |

Each script contains its own safeguards and historical assumptions. Git history
is the authoritative record of subsequent edits.
