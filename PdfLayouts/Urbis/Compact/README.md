# Urbis staged imports

Pass this directory to CompactLayoutImporter or PdfLayoutEngine.SimpleRunner. The runner accepts mixed PDF/XLS/XLSX manifests and levels 1 through 5. Automatic discovery across producer catalogues remains a separate migration step; use PdfLayouts/Urbis/Compact for this release.

## Definitions

- excel-aj-document: journal grouped by Doklad / Spolu za doklad, with final Spolu controls.
- excel-aj-date: journal with document number per movement and date subtotals.
- excel-gl: standard GL columns, 8/9-column export variants.
- excel-gl-dimensions: GL with FK and An.r. dimensions (11 columns).
- excel-af: account framework with A/P, RS, HS and An1–An15.
- pdf-aj, pdf-gl, pdf-af: the three supplied PDF report structures.

Excel layouts declare header predicates, column counts and mappings. Workbook filenames do not control recognition, IČO or fiscal year. XLS and XLSX use ExcelDataReader; the shipped XLSX sample is an Add-In audit workbook and is intentionally rejected. Synthetic OOXML tests verify the XLSX reader path for valid original export structures.

Only single-sheet original exports are accepted. Extra sheets are not silently ignored, even if the first sheet looks valid. No fallback to the old filename-based importer is used. Existing legacy importers and Add-In routing are not replaced by this patch.

## Stages and provenance

L1 reads only the header rows (or first PDF page). L2 obtains GL periods from the header and journal years from transaction dates. Excel AF has no reporting year. L2 retains no movement rows; journal Excel date discovery still scans the sheet. Later stages reopen the Excel stream; PDF pages are cached within the session.

None of the supplied Urbis exports contains a verified IČO in its supported header fields. The importer warns and leaves IČO unset. PDF entity names are retained. Source filenames are retained as provenance only. Passing a selected fiscal year continues to apply journal date-review rules; it does not turn filename hints into source evidence.

At L3, structural rows and original Excel cell text (after legacy XLS character decoding) are retained. Class and group headings in AF are preserved as source rows, excluded from canonical account entries. Three-digit accounts are marked synthetic. GL account duplicates with differing dimensions are retained; FK/An.r. are preserved in SourceDimensions.

L4 checks amounts, missing accounts, document boundaries, document/date/report totals and GL balance equations. Daily totals are checked by date rather than by all rows since the last printed subtotal: single-record dates need not have a subtotal. GL report turnover controls exclude printed subtotals and balance-sheet/P&L summary rows. Values '-' and '*' mean zero where the export uses them; comma and dot decimal separators are accepted. Invalid dates are preserved and excluded for auditor review; invalid amounts and failed controls block canonical publication.

L5 uses the common AJ/GL/AF canonical mapper. Urbis GL has signed net opening/closing values; gross debit/credit balances cannot be recovered. The reported turnover is also retained as period turnover, matching the previous Urbis importer. PeriodFrom/PeriodTo retain the actual reporting interval.

## PDF details

Column coordinates are reference PDF points with header-based scale/offset fitting. Journal document context continues across page boundaries. Printed page numbers are checked. Footer lines beginning Vytlačené are excluded. Source-order numeric extraction separates journal amounts from adjacent description/account text.

The PDF adapter recovers a missing Unicode space only when the TrueType font encoding explicitly maps that source code to space. Other unknown glyphs are not guessed.

## Supplied corpus

14 files pass L2/L5: 11 single-sheet XLS exports and 3 PDFs (221 pages total). Two files are intentionally rejected:

- U_DENNIK_00312959_2025_a.xls: 10 worksheets.
- U_DENNIK_00312959_2025.xlsx: 90 worksheets, Add-In audit workbook.

U_DENNIK_00325392_2025.xls contains 932 transactions dated in 2026. ExpectedYear=2025 must produce an expectation mismatch.

The Opava GL PDF and XLS agree on all 95 canonical rows: accounts, opening/closing net values and debit/credit turnovers. The Olováry journal PDF yields 4,943 canonical entries with validated document and report totals. AF yields 354 Excel and 678 PDF account rows, with headings excluded.
