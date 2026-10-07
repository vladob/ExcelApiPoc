# KROS OMEGA staged imports

Derived from the supplied Dalton exports. These definitions describe the observed
exports, not every OMEGA report or arbitrary Excel workbooks. The Add-In routing
and the existing SoftipMop monthly journal importer are unchanged.

| Definition | Input | Canonical data |
| --- | --- | --- |
| pdf-aj.json | Účtovný denník (zúžený - dátum) | AJ |
| pdf-gl.json | Hlavná kniha (zúžená) v EUR | GL |
| pdf-af.json | Účtový rozvrh | AF |
| excel-aj.json | Single-sheet, 14-column original journal export | AJ |
| excel-gl-unsupported.json | Collapsed print-style GL workbook | Explicit rejection |

## Stages

1. Recognize by title/header and PDF OMEGA footer; Excel uses the exact export
   header and column count. No recognition depends on filenames.
2. Identify: PDF reads one page, using the report year and entity text. Excel AJ
   scans DUÚP dates without retaining source rows to establish observed years.
   None of these samples contains a verified IČO; this produces a warning.
3. Extract rows. Account names/descriptions may continue onto following lines
   or pages; continuation text is restricted to the declared columns. GL has
   separate annual and period turnover columns. Subtotals remain source rows
   and are not imported as ordinary accounts.
4. Check formats and printed controls. Empty money cells mean zero only where
   the definition declares it. AJ keeps independent debit/credit amounts and
   requires an account for each nonzero side. PDF AJ totals must match both
   printed totals. GL checks each synthetic subtotal and the opening/annual
   movements/closing equation. AF retains the printed flags in SourceFields.
5. Map into the shared AJ/GL/AF models with AccountingFormat = KROS OMEGA.

AJ uses DUÚP as PostingDate. Excel also retains due date, accounting month,
cost centre, external document number and partner in SourceFields. Synthetic
and analytical account components are three digits each (82 + 1 becomes
082001); invalid components block normalization. Invalid dates and mixed years
follow the existing auditor-review policy. An explicitly selected fiscal year
selects that year's entries; other entries remain imported but excluded from
calculation. No fiscal year or IČO is guessed from the filename.

The PDF's document numbers and the Excel cell values are preserved independently.
Some Excel document numbers are numeric cells in General format and have already
lost leading zeroes. The importer does not invent their original width.

## Explicitly unsupported samples

* HL_KNIHA_31674453_2024.xlsx: blank amount cells have collapsed in this print
  export. Rows 28 and 31 put different debit/credit meanings in the same Excel
  columns; row 48 shifts the closing balance farther right. A fixed mapping
  would silently misclassify amounts. Use the matching, supported PDF.
* HL_KNIHA_31674453_2025.pdf: image-only scan. The shared compact importer now
  returns `unsupported` with `noReadableText` when the first page has no text.
  A blank later page still stops extraction as an invalid document. No OCR.

The compact PDF importer retains the first and current decoded pages instead
of every page. Source rows and diagnostics remain available, and DecodedPages
counts distinct pages decoded. Direct PdfPageSource callers retain the previous
full-cache behavior unless they opt out.

## Verified supplied data

* GL PDF: 14 pages, 725 canonical accounts; 89 synthetic controls checked.
* AF PDF: 30 pages, 1,877 canonical accounts.
* AJ PDF: 1,823 pages, 131,710 canonical entries; one printed total source row.
* AJ Excel: 131,710 canonical entries.
* AJ debit and credit totals each EUR 158,530,516.57 in both files. All entries
  match by date, accounts, amounts and document number after ignoring leading
  zeroes in numeric document numbers for comparison only (295 entries).

Printed-control checks are extraction checks; cross-file business reconciliation
still belongs to the shared accounting validation stage.
