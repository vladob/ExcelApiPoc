# Step 4: v2 manifest runner

Base commit: `e67e62f` on `feature/ifosoft-oxps-layouts`.
The checked-in corrected `PdfLayoutEngine.Tests/FilesToTest.csv` has 1,039 entries and matches the uploaded CSV's parsed contents. This change does not modify the manifest, engine or definitions.

## Start with ten files

From the repository root in PowerShell:

```powershell
dotnet run --project PdfLayoutEngine.Runner/PdfLayoutEngine.Runner.csproj -c Release -- --allow-review-drafts --limit 10
```

Then run the entire manifest:

```powershell
dotnet run --project PdfLayoutEngine.Runner/PdfLayoutEngine.Runner.csproj -c Release -- --allow-review-drafts
```

The current definitions require the explicit `--allow-review-drafts` switch. Validation defaults to `internalConsistency`, extraction to `records`, and the cooperative per-file deadline to 300 seconds. Each file prints a start and completion line. Ctrl+C cancels the run and preserves completed reports. No production importer or database is called.

Results go under `TestResults/LayoutV2/<UTC timestamp>-<unique ID>/`. The console prints the exact directory. Runs never overwrite one another.

## Outputs

- `summary.csv`: UTF-8 with BOM, semicolon-delimited and quoted, suitable for Slovak Excel. One row per completed manifest entry. Contains expected/actual values, independent statuses, eligible record count, page coverage, orphan/candidate counts, duration, explanations and the detail path.
- `results.jsonl`: one compact JSON object per completed entry, each validated against `manifest-result.schema.json`.
- `files/row-NNNNNN.json`: per-file envelope with resolved path, duration, error type/message/stack trace where applicable, summary and engine-result path.
- `files/row-NNNNNN.engine.json`: full engine result including extracted occurrences, fields, controls, calculations, validation, diagnostics and provenance. Validated against `result.schema.json`. No engine file exists if extraction failed before producing a result.
- `run.json`: selected/completed counts, status counts, options, timestamps and SHA-256 hashes for the manifest and catalog. Updated after every completed file.
- `manifest.csv`: exact input manifest snapshot.

CSV/JSONL are flushed after each file. Per-file JSON and run metadata are replaced atomically. A forcibly terminated process can leave a `.tmp` file or one file whose completed detail precedes its summary entry; preserve the directory and rerun the remaining source rows in a new run.

## Reading the results

The manifest contains expectations, not extraction instructions. The engine receives only the resolved document path and processing options. `PreviousTestResult` is retained as historical text and does not influence scoring.

Category comparison maps `HL_KNIHA` to `GL`, `U_DENNIK` to `AJ`, and `UCT_ROZVRH` to `AF`. Category must be confirmed before comparison. A leading eight-digit CIN in `AccountingEntity` is compared with an extracted, present CIN; a missing CIN is inconclusive. Names remain visible but are not treated as equivalent by fuzzy matching. Non-empty expected years are compared only with a resolved fiscal year derived by the engine.

Explicit disagreement produces `expectationStatus: fail`; missing or ambiguous actual evidence produces `inconclusive`. Processing, recognition, expectation and validation statuses are independent. A detected layout does not imply correct records or complete source material. `EligibleRecordCount` counts only occurrences marked `eligible`, excluding structural headers and controls. It is not production export approval. Document/group calculation results appear under `standardTotals`; record-level equations remain in the full engine JSON. Disabled totals stay `notEvaluated`.

The Step 3 engine's review limitations remain, including unresolved reporting-entity CIN, some block ambiguities, disabled aggregate reconciliation and unknown original-input completeness. Many inconclusive results are therefore expected. The runner records evidence for investigation; it does not claim these have passed.

## Other options

```powershell
# Specific source lines (header is line 1); retain original row numbers:
dotnet run --project PdfLayoutEngine.Runner -c Release -- --allow-review-drafts --first-row 101 --last-row 110

# Longer per-file deadline and explicit output directory:
dotnet run --project PdfLayoutEngine.Runner -c Release -- --allow-review-drafts --timeout-seconds 600 --output F:/Audit/TESTS/V2Results

# Recognition-only triage (validation must be none to avoid promotion to records):
dotnet run --project PdfLayoutEngine.Runner -c Release -- --allow-review-drafts --extraction recognition --validation none

# Same manifest, files relocated to another root:
dotnet run --project PdfLayoutEngine.Runner -c Release -- --allow-review-drafts --source-root F:/Audit/TESTS/TestAll --input-root D:/CopiedTestAll
```

Windows paths require root remapping on Linux/macOS. Relative paths resolve against the manifest directory. Duplicate entries are deliberately retained as separate test cases. Malformed headers, columns, category names or years stop the run before processing. Missing, unsupported or malformed individual documents are reported and the batch continues. Output write failures stop the batch; they are not concealed as document failures.

Timeouts are cooperative: the adapter/engine must reach cancellation checks. They are not operating-system hard process limits. Very large document results can consume substantial memory and disk space because complete provenance is retained. Start with a small batch to measure this on your corpus.

Exit codes: `0` completed without explicit processing/expectation/validation failures; `1` setup or run failure; `2` one or more such per-file failures; `130` cancelled. `--strict` also makes review/incomplete/unsupported/inconclusive outcomes exit `2`. Reports are still written before exit.

## xUnit entry point

The standalone runner is recommended for long runs and immediate progress. An opt-in test is also included:

```powershell
$env:IFOSOFT_V2_RUN = '1'
$env:IFOSOFT_V2_LIMIT = '10'
dotnet test PdfLayoutEngine.Tests/PdfLayoutEngine.Tests.csproj --filter 'FullyQualifiedName~ManifestCorpusTests'
Remove-Item Env:IFOSOFT_V2_LIMIT
Remove-Item Env:IFOSOFT_V2_RUN
```

Optional environment variables: `IFOSOFT_V2_MANIFEST`, `IFOSOFT_V2_OUTPUT`, `IFOSOFT_V2_LIMIT`, `IFOSOFT_V2_STRICT=1`. Without `IFOSOFT_V2_RUN=1`, this test is skipped. By default it asserts that the selected corpus was processed and reported, not that every accounting result passed. Strict mode additionally requires all processing/expectation/validation statuses to pass.

Portable regression command:

```powershell
dotnet test PdfLayoutEngine.Tests/PdfLayoutEngine.Tests.csproj --filter 'FullyQualifiedName!~DiagnosticTests'
```

Verified here: 99 tests passed; one opt-in corpus test skipped. Runner tests cover quoted/multiline CSV, invalid expectations, root mapping, missing/malformed files, per-file timeout handling, cancellation checkpoints, row subsets, duplicate preservation and schema-valid results. A real CLI smoke run also exercised a minimal XPS, malformed PDF, missing PDF and unsupported extension. The full 1,039-file corpus was not executed here because its `F:` files are on your computer.
