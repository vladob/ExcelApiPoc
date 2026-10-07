# Ives compact PDF layouts

Six executable definitions for GL, AJ and AF. These replace the earlier review-only GL JSON drafts. Use this directory with `PdfLayoutEngine.SimpleRunner`; do not combine it with the legacy Ives `.v1.json` directory.

| Definition | Samples |
| --- | --- |
| gl-portrait | Turcovce 2022; Malcice 2024 |
| gl-portrait-summary | Malcice 2023, 2025 |
| gl-landscape | Turcovce 2024 |
| gl-landscape-six-amounts | Vojnatina 2022-2025 |
| journal | All nine supplied AJ PDFs |
| account-framework | All seven supplied AF PDFs |

## Execution

Version 1 IfoSoft definitions retain their existing execution path. Version 2 adds a small `table` definition: two horizontal calibration labels, a last-header label, column intervals in reference PDF points, ordered row predicates, amount columns, optional amount sums, and continuation settings. Font size is not a recognition condition. Vertical table location is rediscovered on each page.

Column labels provide uniform horizontal scale and offset. This is not arbitrary nonlinear column warping. Layouts with materially different column structures still require distinct definitions. Coordinates are reference points, not GMX units.

Recognition matches the report title, orientation where specified, and distinguishing column labels. File names do not establish category, IČO or fiscal year. Level 2 opens only the first page. IČO and reporting dates come from the header; AF has no inferred fiscal year. Later pages may omit the reporting period. Printed page sequence/total and available IČO are checked.

## Records and controls

- GL imports account summaries only. Transactions, synthetic subtotals, dimension subtotals and printed group totals remain source records and are not counted as imported account summaries. Every printed GL group total is compared with its account summaries.
- AJ imports every activity group, including Hlavná činnosť and Stravovanie. Continuations are attached to the previous record, including across a page break. Continuation text and group names remain in SourceFields. Printed group totals are compared with extracted amounts. A missing account on one side is allowed; both sides missing is an error.
- AF starts a record at an account code and appends following name-column lines on the same page. Unassigned content is diagnosed. Account code whitespace is removed; dots and letters remain significant.
- Missing or implausible journal dates are retained and flagged; existing JournalDateReview determines calculation eligibility. Out-of-year source dates generate warnings.
- PDF GL descriptions sometimes physically overlap numeric text. `amountsInSourceOrder` separates numeric sequences in the original PDF text order, rather than interleaving description digits by X coordinate. Missing or invalid imported amounts block canonical output.

For the six-column GL, both printed turnover pairs remain available in source results. `TurnoverDebit/Credit` are the sum of the preceding-to-date pair and the selected-period pair; `PeriodTurnoverDebit/Credit` use the selected-period pair. In the supplied full-year samples the preceding-to-date pair is zero. A non-January or partial-period sample is still needed to verify that interpretation outside this corpus.

Canonical output uses the definition's producer and extracted IČO. Original raw row text, source page and group context remain available. No accounting business-rule engine is introduced here: printed-control comparisons verify extraction completeness.

## Scope and verification

REKAP and ZBORNIK are not recognized by these six definitions. CSV, XML and spreadsheet imports are not migrated by this patch.

Tests cover recognition, independent font sizes, scale/offset, currency placement, overlapping numeric description text, mismatching printed totals, invalid columns and catalogue loading. The supplied PDF corpus is also exercised at L2/L5; see the accompanying result summaries. Verification used local .NET 8 compiler/reflection test execution. Run the normal project test command on the Windows checkout after applying the patch.

Run from the repository root:

```powershell
dotnet test PdfLayoutEngine.SimpleTests/PdfLayoutEngine.SimpleTests.csproj
dotnet run --project PdfLayoutEngine.SimpleRunner -- PdfLayouts/Ives/Compact 2 "F:/Audit/TESTS/FilesToTest-Ives.csv" "F:/Audit/Results-Ives-L2"
dotnet run --project PdfLayoutEngine.SimpleRunner -- PdfLayouts/Ives/Compact 5 "F:/Audit/TESTS/FilesToTest-Ives.csv" "F:/Audit/Results-Ives-L5"
```

The manifest should contain FilePath, ExpectedYear and PerceivedCategory columns, as in the existing runner. Actual file location and result folder are caller-controlled.
