# Ives structured exports

Use the same staged importer and runner with `PdfLayouts/Ives/Compact`.

Supported samples: 17-column semicolon CP1250 journal CSV; Crystal Reports XML journal, detailed general ledger, and account framework. Four small JSON definitions map source columns/FieldName values. XML visual Field.Name values are not identifiers.

CSV structure is shared with an IfoSoft export. Selecting this Ives catalogue supplies the producer context; the column structure alone does not prove the producer. IČO and entity name come from the CSV header, year from posting dates. The supplied U_DENNIK_00323217_202512.CSV contains 2,310 records dated 2026, despite its filename.

Crystal XML is read section by section. Level 1 stops once a matching record is found (at most 4,096 sections). Level 2 scans transaction dates/counts without retaining rows. Levels 3–5 extract, validate, and map into the existing canonical models. Journal continuation fields are preserved. GL imports account summaries, not individual transactions or synthetic subtotals. AJ/GL totals are checked separately for each activity group before canonical publication. All source FieldName/Value pairs of imported records are retained.

XML samples contain no verified IČO or entity name; these remain unset with a warning. AF also has no fiscal year: validity dates and filenames are not used as evidence. AJ/GL years describe observed transaction years, not a separately declared reporting period. Mixed-year journals require an auditor-selected year for calculation eligibility. Invalid dates remain in the journal, flagged and excluded; invalid amounts or failing controls block canonical output.

The supplied HL_KNIHA_00325465_2025.xml is byte-identical to REKAP_00325465_2025.xml and is intentionally unrecognized. The supported detailed ledger is HL_KNIHA_00325465_2025_a.xml. ZBORNIK remains unsupported.

Validation on the supplied XML/PDF pairs: all 6,391 AJ records match on document number, date, both accounts and amounts; all 808 GL records match on account, opening/closing net and debit/credit turnover; all 3,203 AF records match on account and type. XML text may retain content clipped in PDF output.

No CSV GL or AF sample was present in this ZIP, so no unverified Ives mappings for those formats are included.

## IVES Excel report exports

`excel-aj.json`, `excel-gl.json`, and `excel-af.json` run through the same
`CompactLayoutImporter` / `StagedImportRuntime` path as the AddIn. The workbook
reader detects binary XLS or OOXML from its contents, including OOXML files with
an `.xls` extension. Exactly one worksheet is required.

The optional Excel `report` definition declares metadata cells, row classifiers,
allowed populated columns, field mappings, continuation rows, and printed-total
fields. Columns are zero-based; identifier row numbers are one-based. A row must
match exactly one rule. Unknown rows, extra populated cells, orphan continuations,
missing amount rows, invalid amounts, and mismatched controls block normalization.
Raw cells and continuation locations remain available as source provenance.

- AJ: report-header IČO and period; transaction rows followed by module/dimension
  continuations; the printed amount total checks all source amounts, including
  one-sided journal entries. Canonical debit and credit totals use their respective
  account sides.
- GL: account/name rows followed by six-amount rows; transaction detail and
  subtotal rows are classified separately. Only account balances enter the
  canonical GL. All six printed report totals and each account balance equation
  are checked. The report-total debit column differs from the detail debit column
  and has an explicit mapping.
- AF: the flat `Id_uctu` / `aU_Text` table. Account-validity dates are retained as
  source cells; they do not establish a fiscal year or entity identifier.

Private source verification: Vojnatina journals for 2023, 2024, 2025; ledgers for
2024 and 2025; and the 2024 framework. All six pass extraction and printed controls.
The 2024 and 2025 AJ/GL pairs reconcile with zero account differences. The 2024
Excel and PDF journal dates/documents/accounts/amounts and GL account balances
match. The framework exports differ: Excel has 1,984 rows, PDF has 1,997; no rows
are synthesized to make the sources agree.

Optional real-file tests use these environment variables (folders of extracted
source files, not ZIP paths):

```powershell
$env:IVES_VOJNATINA_TEST_DIR = 'F:\path\to\VojnatinaExcel'
$env:IVES_VOJNATINA_PDF_DIR = 'F:\path\to\VojnatinaPdf'
dotnet test .\PdfLayoutEngine.SimpleTests\PdfLayoutEngine.SimpleTests.csproj --filter FullyQualifiedName~IvesExcelLayoutTests
```

Without those variables, only the synthetic tests exercise their fixtures; the
optional private-file test methods return without running corpus assertions.
