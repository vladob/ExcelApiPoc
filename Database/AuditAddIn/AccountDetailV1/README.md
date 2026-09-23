# Account detail helper V1

Run these scripts against the existing `AuditAddIn` database in order:

1. `010_CreateUsers.sql`: set the test auditor's display name and email at the top of the seed batch. The result set contains newly generated API keys for the two users. Save them outside the repository. On rerun, existing keys are not shown or replaced.
2. `020_CreateAndSeedPredefinedTexts.sql`: create category-specific suggestion lists and account defaults for the users in `Settings.Users`, using the auditor's `TextSeed.xlsx` as the authoritative source.
3. `030_CreateAndSeedWorksheetLayout.sql`: create and assign the shared version 1 `ACCOUNT_DETAIL` layout. A user without an explicit assignment receives this default.
4. `040_GrantApiSettingsAccess.sql`: grant the existing IIS application-pool identity read access to identities/layouts and CRUD access only to texts/defaults. Run before deploying the new API endpoints.

These are additive scripts. They do not drop or overwrite existing auditor edits. Do not grant database access to the Excel add-in.

## Seed provenance and unresolved data

- The workbook `TextSeed.xlsx` supplies 13 texts per category and 67 account rows. Of those accounts, 63 have valid defaults in all four categories (252 defaults in total). Account `701` has no selected text. Accounts `693` and `694` refer to undefined text ID `31`, and `697` refers to undefined ID `313` in all four categories; their defaults are deliberately omitted.
- The period-determination list in the original SQL sketch agrees with the workbook, but that SQL reused its mappings for unrelated categories. The workbook's four distinct lists and mappings take precedence. `Rank` in the sketch corresponds to the workbook's `Ocenenie` field, represented here as `Valuation`.
- Seed rows are initially copied to both users. They can maintain their own copies afterward. Rerunning the scripts inserts missing rows only; it does not reset text or defaults changed by an auditor.
- The sample layout is a proposed rendering contract. The add-in must validate its `schemaVersion`, keys, row placement, and allowed styles before use. Audit-goal-derived summary rules are reserved for a later version.

The initial account detail table contains static journal values. The four suggestion cells sit outside it, allow free typing, and use category-specific dropdown lists. Existing account sheets are never rewritten by the create-missing-sheets action.

## API (after step 040)

All routes under `/api/v1/account-detail` require the auditor's `X-Api-Key` header. The API resolves the user from that key; callers never choose a `UserId`.

| Method and route | Purpose |
| --- | --- |
| `GET /texts` | Categories, active suggestions, and account defaults for the caller |
| `GET /layout` | Caller-assigned `ACCOUNT_DETAIL` version, or the shared default |
| `POST /texts` | Add a suggestion (`categoryCode`, `textSk`, optional `sortOrder`) |
| `PUT /texts/{category}/{textId}` | Edit wording and sort order |
| `DELETE /texts/{category}/{textId}` | Delete an unused suggestion; returns 409 while an account default refers to it |
| `PUT /defaults/{account}/{category}` | Set or replace a default with `{ "textId": 1 }` |
| `DELETE /defaults/{account}/{category}` | Clear a default |

Account codes are exactly three digits. The API does not change any previously generated workbook sheets. Newly created books take a snapshot of the retrieved values.
