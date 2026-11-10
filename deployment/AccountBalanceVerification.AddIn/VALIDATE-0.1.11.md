# 0.1.11: formula-driven appointment decrees (source candidate)

This change extends ABV_People with Prvá and Účty boolean cells and independent
Menovací dekrét / Rekapitulácia roles. The decree is a single dynamic-array formula:
changing names, roles, address or dates updates every decree without refresh.
The existing print/PDF output excludes the decree; its own ribbon actions size
its print area, insert page breaks, and export one PDF per recipient.
Rekapitulácia roles are stored, but that worksheet is not implemented yet.

## Build and test on Windows

From the repository root with .NET 8 SDK and the existing add-in build prerequisites:

```powershell
dotnet test .\AccountBalanceVerification.Tests\AccountBalanceVerification.Tests.csproj -c Release
if ($LASTEXITCODE -ne 0) { throw 'Tests failed.' }
dotnet build .\AccountBalanceVerification.AddIn\AccountBalanceVerification.AddIn.csproj -c Release
if ($LASTEXITCODE -ne 0) { throw 'Build failed.' }
```

No build or .NET test run was possible in the authoring environment: it has neither
the SDK nor Excel. This is source for validation, not a tested installer.

## Excel acceptance checks

Use a copy of the supplied workbook; keep the original for comparison.

1. For an existing 0.1.10 workbook, click **Pripraviť starší zošit** once. New
   workbooks already contain the parameters and formula worksheet.
2. Verify all existing names and print selections survived:
   Prvá strana -> Prvá only; Na konci každého účtu -> Účty only;
   Všetky strany -> both. Existing account comments and footer positions remain.
3. Commission roles default to decree recipient / recap commission. Explicitly
   select one decree signer and the recap signer. They may be the same person.
   Choose Členka / Predsedníčka explicitly when those appointment forms apply.
4. Fill the document fields in H10:I16. Decree issue, effective appointment and
   inventory reference dates are independent of the existing report date.
   Order number/date/receipt date are reserved for the recap.
5. For the sample: issue 17.11.2025; effective 01.12.2025; inventory 31.12.2025;
   locality Kamienke; supply the entity address. Use the three commission members
   as recipients and Ing. Benjamín Blaha as the decree signer.
6. Change a recipient name, date and signer name. Confirm the decree updates
   immediately without a refresh command. Add/remove a recipient and confirm the
   spill expands/contracts. Test zero recipients and blank name rows.
7. Native in-cell checkboxes are used where supported. Older Excel builds get a
   TRUE/FALSE list; either representation must drive both signature formulas and
   print selection identically. Excel automatic calculation is expected.
8. Tlačiť dekréty: three separate A4 pages, one complete decree per person,
   correct chair/member wording and signer, no clipping or blank extra pages.
9. Dekréty do jednotlivých PDF: select a folder, then verify the new subfolder
   contains exactly three one-page PDFs. Repeat with two/four recipients.
   Previous exports must remain unchanged, including duplicate recipient names.
10. Normal Tlačiť / Exportovať do PDF: existing title and account sheets only.
11. Check warnings for missing/multiple signers, unknown recipient role, missing
    or invalid dates, an occupied spill cell, or a pasted invalid boolean.

The repeating decree layout supports up to 1,000 recipients. Ribbon output sizes
and paginates it automatically. Excel's native File > Print uses the most recently
set print area; use the dedicated ribbon commands after changing recipient count.
The spill is generated content; edit inputs on Parametre, not its output cells.
