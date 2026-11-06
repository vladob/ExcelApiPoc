# Inventarizácia 0.1.3

## Deploy

1. Pull `feature/account-balance-verification`.
2. Finish POSNOVA synchronization in MkSoftRaw for the client/year being tested.
   The loader must provide `Data.POSNOVA(ClientErpId, FiscalYear)` with `UCET` and
   `NAZOV`. This release does not modify the loader.
3. Execute `Database/AuditAddIn/Migrations/MkSoftVerificationV1/040_Posnova.sql`
   in SSMS on the API's SQL instance. It creates the AuditAddIn wrapper and
   grants the existing IIS user read access. Adapt the principal if necessary.
4. Publish ExcelApiPoc.Api to IIS, preserving deployed configuration.
5. Close Excel and run:
   ```powershell
   .\deployment\AccountBalanceVerification.AddIn\Create-Package.ps1 -Version 0.1.3
   ```
6. Extract the ZIP and run Install.cmd. Create a new inventory workbook.

## Source definitions

The authenticated endpoint is
`GET /api/v1/mksoft/clients/{clientErpId}/years/{fiscalYear}/accounting-framework`.
It uses the existing active API key and `Settings.Users.CanAccessMkSoft` checks.
The general-ledger response now includes POSNOVA definitions as well. The
existing general-ledger function continues to supply PROZVRH analytical names.

Both framework dropdowns are removed. File imports use AF names first and GL
names second, with no standard-framework API call. Explicit synthetic heading
names are retained separately from monetary rows, including AF heading records.
An analytical account's name is never guessed to be the synthetic account name.
Missing names are reported on Parametre. Journal-only inputs can still create a
workbook, but cannot supply names that are absent from their source data.

## Workbook and output

- Titulná strana is the first printable sheet. Account sheets follow by account
  number (leading zeros preserved). Parametre stores dates, location, source
  information, messages and the ABV_People Excel table.
- Function and placement columns have dropdown validation. Default functions
  are editable in Nastavenia and stored in the existing AppData settings file.
  Blank names are omitted; invalid pasted placement values block output with
  an explanatory message. Extend the people table to add rows.
- Prvá strana means title only; Na konci každého účtu means each account's end;
  Všetky strany means title plus each account's end. Nothing is repeated on
  every physical page. Location/date follow signatures.
- Tlačiť and Exportovať do PDF refresh signatures/dates from Parametre, then
  copy only tagged title/account sheets into a temporary workbook in the correct
  order. The temporary workbook is discarded. No administrative or unrelated
  user-created sheet is printed. Source tab order and active sheet are preserved.
- Roles and account codes use worksheet custom properties. Named ranges track
  signature anchors and parameter cells when tabs are renamed or rows inserted.
  Do not delete the generated names, tags or people table. Copied duplicate
  account/title sheets are rejected to avoid unintended duplicate output.
- Ordinary Excel printing remains available but does not perform this refresh
  or selection. Existing 0.1.2 workbooks are not automatically migrated; recreate
  them to use the new output buttons.

## Verification on Windows

Build/tests can run outside Excel. Live COM, printer pagination, PDF export and
SQL execution must be checked on the deployed environment. Test title/account
selection after renaming/reordering tabs, adding an unrelated sheet, changing
people and placements (including removing a person), and an account that spans
multiple printed pages. Confirm signature updates and that the temporary print
workbook closes after preview/export. Verify POSNOVA by client ID and IČO.

## Automated results

- Add-in and API Release builds succeeded.
- AccountBalanceVerification: 24 passed; API tests: 10 passed.
- Shared accounting import suite: 206 passed, 51 failed at their explicit
  prerequisites requesting local sample-file environment variables. Those
  external samples are not present in the build environment.
