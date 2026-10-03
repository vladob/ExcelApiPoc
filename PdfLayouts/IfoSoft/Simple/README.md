# Small IfoSoft layout importer

Baseline: `8cabdab7d2a607a9a7af3ebce6b4fe7aa5ea46e3`.
This is an opt-in replacement path, initially for `_HLKNIA4.GMX` only.
Existing importers and AddIn selection behavior are not switched automatically.
The later v2 catalog, schemas and rule graph are not dependencies.

## Layout

`hlknia4.json` is a single 59-line definition. Coordinates retain GMX units
(0.1 mm, 254 units/inch), top-left origin. Adapters expose upright page-local
points; the engine calibrates the printed header frame to establish scale and
translation. Keeping source units makes comparison with GMX and the Power Query
workbook direct. It does not assume nominal page margins or 100% print scale.

Header coordinates are page-relative. Body rectangles describe relative positions
within a row/block, not fixed page Y coordinates. The engine groups nearby
baselines, classifies the left side, then reads each row's horizontal ranges.
The six amount columns are shared. Report-footer fields are located relative to
the total row. The first implementation supports horizontal rows and uniform
scale/translation, not arbitrary skew or OCR.

Row rules are evaluated in JSON order:

- Report/class labels take precedence; `***`/`**` must match the label range
  exactly when used without their descriptive label. Asterisks inside an account
  such as `357***` are preserved.
- The account font size and bold/regular weight distinguish synthetic and regular
  rows. Size is compared after calibration; font families are recorded as source
  information, not mandatory matches because printers can substitute fonts.
- Any populated item field makes a regular row an ItemRow; otherwise AnalyticalRow.
- Each row uses its own account rectangle. Internal account spaces are preserved.
- Synthetic, class and report totals are retained as source rows, excluded from
  canonical detail. No numeric-code-length heuristic is used.

## Stages and reuse

| Level | Operation |
|---|---|
| 1 | Match marker and title, inspecting at most three pages |
| 2 | Extract first-page entity/period/metadata; page count comes from document index |
| 3 | Decode all pages, extract PageLevel/body/ReportLevel fields |
| 4 | Parse amounts, check periods, identifiers, page sequence and unresolved rows |
| 5 | Map validated detail to the existing GeneralLedgerImport model |

Levels are cumulative. `CompletedLevel` records stages executed, while `Status`
reports completed/invalid/unrecognized. Amounts are not parsed at level 3.
No business reconciliation engine runs as part of this importer.

```csharp
using var importer = new SimpleLayoutImporter(filePath, layoutPath);
var identification = importer.Examine(ImportLevel.Identify, cancellationToken);
// Keep the same session while the user reviews identifiers.
var full = importer.Examine(ImportLevel.Normalize, cancellationToken);
GeneralLedgerImport canonical = importer.Canonical; // null if extraction/validation failed
```

A session owns the file handle and decoded-page cache; dispose on removal/cancel.
It is single-caller, not thread-safe. Results are cumulative mutable session results.
For the AddIn, populate filenames immediately, then run Examine on a background
worker. Updating the UI and wiring Continue are a separate integration change.
No full-file hash is computed at levels 1–4; level 5 computes the canonical hash.
The XPS page index is read eagerly, while font/page content is decoded lazily.
A requested page is decoded once and reused between stages. A recognition page
is still decoded in full; there is no claim of header-only parsing within a page.

## Data interpretation

- CIN remains null: the GMX has no verified CIN field. Never infer it from filenames.
- Accounting periods 00 through 14 are accepted. Keep the exact period strings;
  canonical ThroughMonth currently carries the printed end-period code.
- Blank monetary cells mean suppressed zero **for this layout**, explicitly set
  by `formats.blankAmount`. Raw blanks remain visible in the source result.
- Amounts accept decimal comma, grouped dots, negative values and `1.440,-`.
- SourceOpeningNet/SourceClosingNet preserve the printed net balances. Existing
  debit/credit properties contain the corresponding positive/negative net split,
  not reconstructed gross balances.
- SourceDimensions retains ZAKC/KP/KZ/T/STR/ZAK without guessing domain mappings.
  SyntheticCode/AnalyticalCode are not guessed from code length.
- Explicit Unicode is retained even when the source clips glyph outlines; this
  yields a warning. Missing Unicode/unsupported decoding prevents clean validation.
- Original completeness and accounting validity are not established by structural
  success. Printed totals are retained for a separate business validator.

## Run

From the repository root:

```powershell
dotnet test PdfLayoutEngine.SimpleTests/PdfLayoutEngine.SimpleTests.csproj

dotnet run --project PdfLayoutEngine.SimpleRunner -- `
  PdfLayouts/IfoSoft/Simple/hlknia4.json 2 F:/Audit/TESTS/HLKNIA4 TestResults/HLKNIA4-L2

dotnet run --project PdfLayoutEngine.SimpleRunner -- `
  PdfLayouts/IfoSoft/Simple/hlknia4.json 5 PdfLayoutEngine.Tests/FilesToTest.csv TestResults/HLKNIA4-L5
```

The input can be one file, a flat directory, or a semicolon manifest containing
`FilePath`. Other manifest columns are not used as evidence or compared yet.
Outputs: summary.csv and numbered per-file JSON including source and canonical
records. Errors are logged separately and processing continues. Exit 2 means at
least one error/invalid/unrecognized input; exit 0 means all inputs completed.
A corpus containing other GMX layouts is expected to include unrecognized results.

Optional real-file regression (extract the supplied GL_xps.zip into a flat folder):

```powershell
$env:IFOSOFT_SIMPLE_TEST_ROOT = 'F:/Audit/TESTS/HLKNIA4'
dotnet test PdfLayoutEngine.SimpleTests/PdfLayoutEngine.SimpleTests.csproj
```

Without this environment variable the real-file test does not execute its fixture
assertions. Portable generated-PDF and parsing tests still run. Real source files
are deliberately not committed.

## Verification on supplied fixtures

| Source | Pages | Detail rows | Debit and credit turnover, each |
|---|---:|---:|---:|
| Adidovce 2023 | 1 | 24 | 0.00 |
| Adidovce 2024_1 | 6 | 262 | 602,463.11 |
| Stakčínska Roztoka 2024 | 9 | 480 | 714,592.70 |

The regression asserts these totals against printed controls; the import runtime
keeps business checks separate. Source markers for HLKNIA4D and PREDVAS3 are rejected.
Real HLKNIA4 fixtures here are OXPS. PDF decoding is covered by generated fixtures
including scaling, translation and rotation normalization. A real HLKNIA4 PDF and
XPS should be added before production rollout. No claim is made about other layouts.
