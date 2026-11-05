# Inventarizácia 0.1.2

This update separates MkSoft and file input into tabs and moves connection settings
and the include-empty-synthetic-accounts option into Nastavenia. Existing saved
API URL/key settings remain compatible.

## Deployment order

1. Pull `feature/account-balance-verification`.
2. In SSMS, execute
   `Database/AuditAddIn/Migrations/MkSoftVerificationV1/030_EntityLookup.sql`.
   It creates two read-only wrappers in AuditAddIn and grants the existing IIS
   database user access to the underlying company metadata in MkSoftRaw. The
   script assumes `IIS APPPOOL\ExcelApiPoc.Api` already exists as a database user
   in both databases, as required by the existing deployment. Adapt the grant
   principal if the deployed API uses another identity.
3. Publish ExcelApiPoc.Api to the existing IIS application. Preserve the deployed
   configuration. The endpoint uses the existing active API key and
   `Settings.Users.CanAccessMkSoft` authorization; no per-client list is required.
4. Close Excel, then run:
   ```powershell
   .\deployment\AccountBalanceVerification.AddIn\Create-Package.ps1 -Version 0.1.2
   ```
5. Extract the generated ZIP and run Install.cmd. Test locally before distributing
   the package to the auditor.

## Workflow

- MkSoft: enter year and client ID or IČO, click **Načítať údaje**, review the
  populated IČO/name/framework, then click **Vytvoriť**. Clicking Vytvoriť before
  lookup loads metadata first and leaves it available for review. Changing the
  year or client ID invalidates the previous lookup. The reviewed IČO, name and
  framework remain editable.
- IČO lookup considers only current company records for the requested year whose
  client agenda contains P or J. Combinations with S or M qualify if they contain
  P/J; payroll-only M does not. Unexpected multiple eligible results require an
  explicit client ID. The API never silently chooses the first result.
- Súbory: select journal and/or ledger, optionally a chart of accounts. Supported
  file contents supply identity/year/name; recognized filenames supply missing
  IČO/year. Conflicts are displayed. Manually edited values are not automatically
  overwritten. Review the accounting system and select the account framework.
- Chart import supplies account names, not balances. Official framework names
  take precedence for synthetic headings, with chart synthetic names as fallback
  when an official definition is absent. POSNOVA/loader changes are not included.
- Nastavenia: Uložiť persists URL, API key and the empty-account option; Zrušiť
  discards edits. Files still require API access for official account definitions.

## Validation

API and add-in compile in Release. All 17 AccountBalanceVerification test cases
passed using direct invocation with xUnit assertions because the standard VSTest
host fails in the build environment. Create-Package.ps1 still requires normal
`dotnet test` to pass on Windows. Live Excel dialog interaction and the SQL
migration must be verified on the target Windows/SQL Server environment.

Check lookup by ID and IČO, a missing year/client, saved settings and cancellation,
file metadata with leading zeros, manual corrections, optional chart import,
and coexistence with the API PoC add-in. Workbook activation before the dialog
and the installer preservation of other add-ins are unchanged.
