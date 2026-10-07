# Performance materiality

`PerformanceMateriality.xlsx` is the supplied `SA320_1.xlsx` reference, embedded in
ExcelApiPoc.AddIn. Workbook creation copies its ISA320 and Metadata sheets together,
renames them to `PerformanceMat` and `__PerformanceMatMetadata`, and makes the
metadata sheet very hidden. The five original tables, formatting, percentages,
and validation controls are retained. XLOOKUP formulas are replaced with scalar
INDEX/MATCH formulas for compatibility. All illustrative ValuesOverview amounts
are replaced before the workbook is handed to the user.

## Official values

The resolver uses the already loaded official RegisterUZ accounting-entity package
(the same source as the RegisterUZ worksheets); it makes no extra service request.
It uses the calculation-selected statement for the audited year. For the previous
year, it uses the uniquely approved annual statement if available, otherwise the
latest submission (statement ID breaks ties). Consolidation scope follows the
selected statement. Without a selected statement, individual statements are used.
Only complete January–December statements are eligible, matching workbook creation.

ListOfTablesFull.Name uses `<template ID>_<one-based table number>`. RegisterUZ
TableOrdinal is zero-based. The mapping's Row is a printed RowNumber, which must
be resolved through the official template to RowOrdinal. Current-period data
columns use the same header resolution as the multi-year reports, including net
assets rather than gross assets. The preceding year's own report supplies its
current-period amount; comparative columns are not silently substituted.

All applicable rows for a source within a report are added together. Different
filings or competing templates are not added together. Missing mappings, missing
rows, duplicate values, ambiguous reports, and non-EUR currency produce #N/A with
an explanatory comment. Zero is reserved for an actual official zero. The hidden
PerformanceMatEvidence table retains the year, report ID, mapping key, printed row,
row ordinal, data-column ordinal, and each amount. ValuesOverview formulas sum those
hidden official amounts, so downstream formulas and the named range update normally.

## Workbook significance

The workbook-scoped `_WB_significance` name points to the Hodnoty cell of the
ImplementationSignificance row labelled `Hodnota z vykonávacej významnosti`
(B53 in the supplied template). New Navigation sheets contain no significance
input. Existing workbooks without PerformanceMat retain their legacy behavior.
Account-detail formulas continue to use the same named range.

## Template issues to review before release

- RevenueSideBudget / Príjmová časť rozpočtu has no mapping in the supplied metadata.
- Modern combined templates such as 687 and 699 have no supplied mappings.
- The supplied 11_1 entries refer to rows 136 and 140 as income/profit, whereas the
  repository's template catalogue identifies template 11 table 1 as assets.
- Several ProfitBeforeTax mappings have descriptions referring to profit after tax.
- The reference's B50 (`Percento - stanovené`) is an independent input; its original
  B49 formula selects Kritická 100% from SignificanceCurrentYear. That behavior is
  retained, as are the reference's default rates and performance percentage.

Mappings are preserved as supplied. Do not silently replace a missing or suspect
mapping with calculated GL data, another report row, a sample amount, or zero.

## Windows release checks (target 1.2.0)

1. Run `dotnet test .\PdfLayoutEngine.SimpleTests\PdfLayoutEngine.SimpleTests.csproj`.
2. Build the add-in in Release and create fresh Vojnatina IVES and Lukacovce IfoSoft
   workbooks, including the no-calculation creation path where applicable.
3. Check all five tables, dropdowns, formatting, hidden metadata, and current/prior
   values against the matching official reports. Check one multi-row mapping.
4. Verify `_WB_significance` refers to PerformanceMat B53; change the selected source
   and performance percentage and check that account-detail significance updates.
   Navigation must have no significance input and must list PerformanceMat.
5. Save, close, and reopen the workbook. Confirm no external-template link, repair
   prompt, or #REF! name and confirm formulas calculate with the add-in unloaded.
6. Confirm AF without a stated year raises no year warning, filename fallback is
   described correctly, and AJ/GL year conflicts still block as before.
7. Review the mapping issues above, then decide whether to bump the currently
   unchanged add-in version to 1.2.0 and publish a release.
