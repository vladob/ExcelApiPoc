# Compact IfoSoft layouts

This is the current opt-in importer. The earlier `IfoSoft/Simple` prototype remains for regression tests; the runner uses this catalogue when given this directory.

## Supported definitions

GL: HLKNIA4, HLKNIA4_V2, HLKNIA4B, HLKNIA4C, HLKNIA4D, PREDVAS3.
AJ: DENNIK, DENNIK_V2, DENNIK1, DENNIK1_V2.
AF: UROZVRH, UROZVRH_V2. OBR_UCET is intentionally excluded.

Definitions follow the reviewed DecipherGMX_Short(8) workbook. Rectangles are left/top/right/bottom in GMX units (254 per inch). Header anchors calibrate scale and placement. Body rectangles are relative to each recognized row or section, not absolute page positions. Shared amount columns stay independent of row-specific account widths. Font weight is checked only at discriminating body anchors. Fixed labels identify class and report totals. Source section names retain provenance.

## Stages

1. Recognize a unique layout from the first page.
2. Read printed identifiers and document metadata from that page.
3. Extract all pages into source rows, preserving printed subtotals separately.
4. Validate formats, account presence, page consistency and unresolved numeric rows.
5. Map accepted data into existing GL, AJ or AF models.

Use `CompactLayoutImporter(file, layoutFileOrDirectory)` and `Examine(ImportLevel, cancellationToken)`. Reuse the importer to advance stages: PDF/XPS adapters cache decoded pages. Dispose it when finished. Business-rule validation is deliberately separate. This patch does not change the AddIn selection workflow or replace its existing production importer.

## Run

From the repository root, with .NET 8 SDK:

```powershell
dotnet test PdfLayoutEngine.SimpleTests/PdfLayoutEngine.SimpleTests.csproj
dotnet run --project PdfLayoutEngine.SimpleRunner -- PdfLayouts/IfoSoft/Compact 2 F:/Audit/TESTS/FilesToTest.csv F:/Audit/Results-L2
dotnet run --project PdfLayoutEngine.SimpleRunner -- PdfLayouts/IfoSoft/Compact 5 F:/Audit/TESTS/FilesToTest.csv F:/Audit/Results-L5
```

The input can also be one report or a flat directory. The manifest is semicolon-separated, with `FilePath` and optional `ExpectedYear`, `PerceivedCategory`, `AccountingEntity`. Relative paths resolve against the manifest directory. Each report produces JSON plus a row in summary.csv. Category and year expectations are checked when available. AccountingEntity is retained as input, not treated as verified CIN. Use a separate output directory per run. A nonzero exit indicates failures or expectation mismatches.

## Evidence and limits

All 12 definitions have generated-fixture tests. Real samples cover HLKNIA4, HLKNIA4B, HLKNIA4D, PREDVAS3, DENNIK1 and UROZVRH. The other six require representative reports before production use. The supplied 11 unique reports completed stage 5 without errors. Nine GL reports reconcile both debit and credit turnover with printed totals; the AJ sample also reconciles. These comparisons are test evidence, not business validation inside the importer.

Clipped source text remains a warning. Missing entity/year is a warning; caller confirmation may be needed. No verified CIN locator is available: filenames and licensed-user labels are not substituted. AF reports may omit the fiscal year. Net balances are preserved as net source values and mapped to one-sided debit/credit values; absent amount columns are listed through SourceAvailableAmountFields rather than inferred as printed zero. AJ and AF source fields preserve dimensions/flags not yet mapped semantically. Unrecognized or invalid rows prevent canonical output. Additional report variants, page-split records and different fonts need real-file testing.

## PDF corpus correction 1

The catalogue now also contains `predvas3_firma.json`: an observed PREDVAS3 print variant with a Firma header, different left-side/header coordinates, and bold subtotal names instead of bold subtotal account codes. Its reference rectangles are PDF points (`unitsPerInch: 72`); scaling and translation still calibrate from header labels. Color is not used for recognition. The optional anchor `numeric: false` permits the bold name to be the discriminator. Original GMX-derived definitions are unchanged.

PDF font detection honors explicit Bold font names when metadata omits the weight. AF rows with `****` analytical markers are retained as AccountHeading, or EmptyAccount when both code and name are absent, and excluded from canonical posting accounts. Indexed vertical lookup, diagnostic deduplication and source-row numbering remove repeated linear scans.

With IFOSOFT_REPAIR_TEST_ROOT pointing to the six supplied PDF directory (using the uploaded filenames), the additional regression test checks bold glyphs, AF placeholders and the observed GL variant. IFOSOFT_SIMPLE_TEST_ROOT continues to run the eight-file GL corpus reconciliation. Without those environment variables, external sample checks return without opening reports; generated fixture tests still run.

Business reconciliation remains separate: a successful stage 5 does not guarantee that printed report totals agree with the extracted entries. The 1,580-page Medzilaborce journal prints a debit total of 29,217,965.57 and a credit total of 129,217,965.57; extracted entries sum to 129,217,965.57 on both sides. These printed values are preserved, not repaired.
