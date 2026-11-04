# Account Balance Verification — initial implementation

Base: release/1.2.0-runtime-integration, 6522d493.

## Projects

- AccountBalanceVerification.Core: DTOs and worksheet selection; all six decimal accounting fields retained.
- ExcelApiPoc.MkSoft.DataAccess: shared server-side repository, parameterized SQL through AuditAddIn only.
- ExcelApiPoc.MkSoft.Client: shared HTTP client using X-Api-Key.
- AccountBalanceVerification.Import: adapter over the existing staged imports and journal/ledger reconciliation.
- AccountBalanceVerification.AddIn: independent Excel-DNA .NET Framework 4.8 add-in, ribbon and settings.
- AccountBalanceVerification.Tests: inclusion, preservation, duplicate and HTTP contract tests.

## Database deployment

Run Database/AuditAddIn/Migrations/MkSoftVerificationV1/010_CreateWrappers.sql on the server with AuditAddIn and MkSoftRaw. It creates/updates only AuditAddIn.MkSoft wrappers. MkSoftRaw, its loader and its queues remain unchanged. The API SQL identity needs SELECT on the wrappers, OfficialAccounts and appropriate underlying MkSoftRaw objects. No permission grants or server configuration changes are made automatically.

The legal-form mapping is the supplied 119 -> 3, 801 -> 2, otherwise 1. Verify these IDs against deployed account frameworks before production use: the example framework names did not match the example entity. EntityLegalForm intentionally exposes only the known PRAVNAFORMA field; entity name and IČO are entered in the creation form pending the final entity metadata contract.

## API

GET /api/v1/mksoft/clients/{clientErpId}/years/{fiscalYear}/general-ledger?includeEmptyAccounts=false

Uses the existing AuditAddIn connection string and API-key user lookup. Configure explicit permitted client IDs per existing user ID, for example:

```json
{
  "MkSoft": {
    "ClientAccess": {
      "123": [11, 895]
    }
  }
}
```

Replace 123 with the actual Users ID. Missing access configuration returns 403. Invalid keys return 401. Missing entity returns 404. No keys belong in repository configuration. This is an initial explicit access policy; replace with a database-backed assignment repository if required later.

The wrapper always limits classes to 0–4. includeEmptyAccounts affects detailed ledger rows. The add-in always requests false. Its separate UI checkbox adds empty official three-digit sheets without adding empty detail rows.

## Excel

Run deployment/AccountBalanceVerification.AddIn/Build.ps1 on the Windows development machine, then open the generated 64-bit XLL. This initial build uses an unpacked XLL: retain every DLL/config file in its output folder. Installer and packed release packaging are not included yet.

The Inventarizácia ribbon opens a creation form for MkSoft or files. API settings are stored separately under %APPDATA%/AccountBalanceVerification/settings.json, matching the existing text-key approach. The workbook is a new unsaved workbook; save it as .xlsx. It requires no add-in to read or edit comments afterward.

Accounting data are initially written as constants, and all worksheets remain unprotected so auditors can edit data and layout. Tables allow filtering and copying. Native table Total Rows use Sum for opening balance, MD movements, Dal movements and closing balance; labels match the worksheet 343 reference (Total / Spolu (zobrazené účty)). Totals respond to filtering. Comments, synthetic notes and responsible-person/signature cells remain editable. Account codes and names are written as text, preserving leading zeros and preventing formula interpretation. Comments use ordinary cells, not Excel threaded comments. No refresh/replacement of existing workbooks is implemented, so existing user comments cannot be overwritten by regeneration.

The introductory worksheet is provisional. Captions use Obrat MD / Obrat Dal. Inventory-confirmed amounts and differences are omitted pending the decision on their behaviour. Physical signature fields are text only; attachments and digital signatures are deferred.

File mode accepts journal only, ledger only, or both through the existing import library. Journal-only requires opening records; journal+ledger rejects reconciliation differences. Ledger-only uses annual turnover, consistent with the existing reconciliation service. Duplicate account rows are rejected rather than silently summed; dimensional ledger exports may require normalization before use. File mode fetches the selected PROFIT / NONPROFIT / GOV_LOCAL framework through the existing applicable-framework API, including empty synthetic sheets when requested. Select the correct framework in the creation form. Journal-only data may lack analytical names. Import warnings are carried onto Úvod. This does not silently assert source completeness.

## Verification on Windows / SQL Server

1. Run the tests and builds with Build.ps1.
2. Execute both wrappers for client 11/year 2025; compare values with the underlying TVFs and confirm only classes 0–4.
3. Check false omits only rows where all four displayed quantities are zero; true retains any additional source rows.
4. Test API invalid key (401), unassigned client (403), valid assigned client (200), absent entity (404).
5. Load both XLLs in Excel 365 x64; check independent ribbons and settings.
6. Create a workbook; test leading-zero/alphanumeric codes, movements with zero closing, empty synthetic option, filtering, copying, editing comments and amounts, and native Table Total Row settings.
7. Save/reopen .xlsx without the add-in and confirm comments, filtering and native table totals remain and all sheets are unprotected.

No live SQL Server or Excel execution is available in the build environment. Import completeness is not yet exposed by the database contract; each MkSoft workbook explicitly records that it has not been verified.

## Build-environment results

- Release builds succeeded for the API, shared libraries and separate net48 add-in; Excel-DNA generated the 64-bit XLL.
- Seven test cases passed, including the existing Urbis GL fixture (annual debit 330, credit 205 and negative correction -30).
- VSTest failed during test-engine startup in this environment. The seven xUnit test cases were invoked directly by a temporary .NET console runner, retaining their actual xUnit assertions. Run the normal dotnet test command on Windows as part of deployment validation.
- No database migration, live API deployment or Excel session was performed.
