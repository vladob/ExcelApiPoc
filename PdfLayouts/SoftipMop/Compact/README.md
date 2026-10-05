# Softip-MOP compact imports

Supported samples: original single-worksheet journal XLSX exports, journal PDF printouts, GL PDF (`UCT-S-201_31_0_1`), and AF PDF (`UCT-S-201_0_0_151`). Seven definitions: one XLSX AJ, four printed AJ variants, one GL, one AF. This is distinct from KROS OMEGA.

## Recognition and extraction

- XLSX: first four header labels identify the export; remaining fields map by header name. Optional columns can move or be absent. Duplicate headers, missing required fields, and multiple worksheets are rejected. Additional original-export columns remain in source fields.
- AJ PDF: DEKORT landscape and Klas portrait variants derive column boundaries from the actual header positions. Required header names, order and the complete panel arrangement must match; unknown/missing header fields are rejected. Landscape exports fit one panel. Portrait exports print all pages of the left panel, then all pages of the middle panel, then the right panel. Join only when panel headers, page counts, row counts and baselines agree. An explicit middle-panel alternative handles the observed optional cost-centre column. Do not concatenate these pages as independent records.
- GL: five-digit accounts are imported. Synthetic totals validate each account group across all six amount columns; opening + annual debit - annual credit is checked against closing. A subtotal label can end one page and its amounts begin the next. Class/report summary rows are not canonical account rows.
- AF: preserve synthetic and analytical account rows; wrapped names continue within their account record.
- L1 recognizes; L2 identifies without extracting body rows (dynamic portrait recognition reads the three panel-header pages); L3 extracts; L4 validates; L5 maps to canonical data. Large PDFs still incur document-opening costs before first-page recognition.

## Dates, identity and provenance

AJ `Účt.mes.` (YYYYMM) determines the fiscal year and period. L2 uses the first nonempty period; L4 verifies every row has that period. `Dát.účt.` remains the posting date. Invalid dates and dates outside the fiscal year are retained for review and initially excluded from calculation. Dates outside the month but within the same year are reported and retained for calculation. Do not change dates to force agreement.

The single `Účet` supplies the account for each nonzero debit/credit side. Negative amounts and leading zeroes in accounts/document identifiers are retained. Rows with both amounts zero remain in source results as `ZeroPosting`, but do not become canonical postings. `DD` is document type; `Stredisko` is cost centre; `St.` is a separate source field.

No verified IČO is present in these samples. AJ worksheet exports also lack an entity name. Keep those identifiers unknown, report the limitation, and associate with the entity selected by the auditor. Filenames are not identification evidence. GL/AF names come from report headers.

AJ worksheet printouts have no printed control totals. Their source amounts can be parsed and validated, but external reconciliation is still required. PDF/XLSX comparison of the five supplied pairs matched every canonical posting on date, document type/number, accounts, amounts and cost centre.

## Monthly XLSX files

The existing monthly API remains available. Opt into the staged engine with:

```csharp
var journal = new SoftipMopMonthlyJournalImporter()
    .Import(monthlyFilePaths, "PdfLayouts/SoftipMop/Compact");
```

It sorts by accounting month, rejects duplicate periods and mixed fiscal years, preserves source filename/row references, and reports gaps between selected months. No Add-In routing was changed. Source files have no verified entity identifier, so the caller must establish that the monthly files belong to the same entity.

## Running tests

From the repository root:

```powershell
dotnet test PdfLayoutEngine.SimpleTests/PdfLayoutEngine.SimpleTests.csproj
dotnet run --project PdfLayoutEngine.SimpleRunner -- PdfLayouts/SoftipMop/Compact 2 F:/Audit/TESTS/FilesToTest_SoftipMop.csv F:/Audit/Results-SoftipMop-L2
dotnet run --project PdfLayoutEngine.SimpleRunner -- PdfLayouts/SoftipMop/Compact 5 F:/Audit/TESTS/FilesToTest_SoftipMop.csv F:/Audit/Results-SoftipMop-L5 --timeout-seconds=600
```

The per-file examination timeout defaults to 120 seconds; the optional override is 1–3600 seconds. Source opening and result serialization are outside that examination timer. The 2,928-page sample took about two minutes for opening and examination in our validation environment; use the larger limit for this corpus. Detailed result JSON can be large.

The supplied Klas GL XLSX has six worksheets, including older/combined material, and is intentionally rejected. No general GL XLSX layout is inferred from that workbook. Only demonstrated export structures are supported.

OMIDA AJ PDFs are outside the supported corpus by agreement; use its original monthly XLSX exports. Their older layout definition is retained for compatibility, but no further OMIDA PDF variants are being added. Combined-month XLSX support is unchanged: the current layout requires a single accounting period per file.
