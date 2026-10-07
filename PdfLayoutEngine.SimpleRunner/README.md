# Unified staged corpus runner

Run from the solution root. Pass `PdfLayouts` to discover all five migrated producers (IfoSoft, IVES, Urbis, KROS OMEGA, Softip-MOP), or keep passing one producer's `Compact` directory to restrict the catalogue. A single JSON layout is also accepted.

```powershell
dotnet test PdfLayoutEngine.SimpleTests/PdfLayoutEngine.SimpleTests.csproj

dotnet run --project PdfLayoutEngine.SimpleRunner -- PdfLayouts 2 "F:/Audit/TESTS/FilesToTest_All.csv" "F:/Audit/Results-All-L2" --timeout-seconds=600

dotnet run --project PdfLayoutEngine.SimpleRunner -- PdfLayouts 5 "F:/Audit/TESTS/FilesToTest_All.csv" "F:/Audit/Results-All-L5" --timeout-seconds=600
```

Levels remain 1 recognition, 2 identifiers, 3 extraction, 4 validation, 5 canonical mapping. Files are processed sequentially; failures do not abort the batch. The timeout is cooperative and starts before importer construction, but synchronous file opening and result serialization cannot be interrupted by it. Default is 120 seconds; accepted range is 1–3600.

## Input manifest

Existing semicolon-delimited manifests remain valid. `FilePath` is required. Optional columns:

- `ExpectedYear`: four-digit year; checked against source-derived fiscal year.
- `PerceivedCategory`: AJ / U_DENNIK / DENNIK, GL / HL_KNIHA, AF / UCT_ROZVRH / UROZVRH.
- `AccountingEntity`: retained context; not used to invent a source entity identifier or asserted as an entity match.
- `ExpectedSoftware`: expected producer. `Software` is accepted as a fallback column name. Spaces, punctuation, and case are ignored; Omega and KROS OMEGA are equivalent.

Example (replace paths with existing files):

```csv
FilePath;ExpectedYear;PerceivedCategory;ExpectedSoftware
F:/Audit/TESTS/TestAll/entity-a/journal.xlsx;2025;AJ;Urbis
F:/Audit/TESTS/TestAll/entity-b/journal.xml;2025;AJ;Ives
F:/Audit/TESTS/TestAll/entity-c/ledger.pdf;2024;GL;IFO soft
```

Expectations never select a layout or supply missing identifiers. Relative paths resolve against the manifest directory. An optional fifth positional argument selects the calculation fiscal year, as before; it does not change the expected-year recognition check.

A directory input is scanned recursively. Every file is reported, including unsupported extensions. The designated output directory is excluded from discovery. Prefer a manifest and a fresh output directory for each run; old JSON files in a reused output directory are not deleted.

## Discovery

A repository/producer root searches its descendant `Compact` directories, excluding old Simple and GMX-era definitions. Flat catalogues and explicit files remain supported. The runner checks for empty catalogues and duplicate layout IDs before starting.

The file extension selects the technical reader. Content selects the layout and producer. XML routing inspects the document root, so Crystal Reports and IfoSoft XML coexist in one catalogue. PDF recognition shares one page source among candidate layouts. Ambiguous cross-producer PDF matches are not resolved by catalogue order.

**Known unavoidable ambiguity:** `ifosoft-csv-aj` and `ives-csv-aj` currently have identical recognition signatures and mappings. With both loaded, those AJ CSVs are `ambiguous`, with both candidate IDs reported, and no canonical output. Supplying ExpectedSoftware does not override this. If provenance is independently known, a separate producer-scoped run remains available. Other distinct IfoSoft CSV variants can still be recognised globally.

Unseen Urbis exports may be `unrecognized`; adding them to the manifest does not imply their layouts are already supported. No new producer-specific layout is introduced by this patch. MkSoft has not been migrated.

## Reports

`summary.csv` preserves its original first 14 columns and appends:

- `Software`, `Format`, `ExpectedSoftwareCheck`
- `IssueCodes`, `Message` (first five issue messages plus expectation differences)
- `CandidateLayouts`, `CanonicalRows`

Each input produces one numbered `.json`, including failures. Its top-level `status` agrees with the summary, while `result.status` describes engine execution independently of expectations. JSON contains all issues, candidate IDs, expectations, exception details where applicable, and canonical data when available. Failed inputs now use the same `.json` shape rather than separate `.error.json` files.

Statuses:

| Status | Meaning |
|---|---|
| completed | Requested stage completed; review warnings separately |
| expectationMismatch | Stage completed, but a supplied expected year/category/software disagrees |
| unrecognized | No supported layout matched; software/category may remain unknown |
| ambiguous | Multiple indistinguishable candidates; none selected |
| unsupported | Unsupported extension, image-only PDF, or recognised unsupported export |
| invalid | Extracted data failed validation; canonical output withheld |
| timeout | Cooperative examination deadline reached |
| error | Missing file, malformed source, or other exception; details retained |

Validation/read failures take precedence over expectationMismatch; individual expectation columns remain available. Missing source identification is `unavailable`, not an invented match. Exit code is 0 if every input completed, 2 if any input requires review, and 1 for usage errors. Invalid configuration/manifest errors abort before the batch.
