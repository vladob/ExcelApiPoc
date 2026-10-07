# Version 1.2.0 validation

This branch incorporates preserved WIP commit fd33453 and connects the AddIn to
CompactLayoutImporter through StagedImportRuntime. The two historical lenient-CSV
commits are not cherry-picked; their examples are covered at the runtime boundary.
Assembly and file versions are now 1.2.0.0; Settings displays 1.2.0.
The larger corpus run is deferred at the user’s request on 2026-10-07.

## Windows automated gate

From the repository root, using PowerShell with Visual Studio Build Tools and .NET 8:

```powershell
.\deployment\ExcelApiPoc.AddIn\Test-ReleaseCandidate.ps1
```

The script loads LocalTests.runsettings by default for both suites. Use
-RunSettingsPath to select another settings file; its external sample paths must exist.
The script stops on failed tests/build/dependency checks. Both staged and accounting
regression suites must pass. Corpus tests that return early without their environment
variables are NOT evidence that the external corpus passed. The larger corpus run is deferred; its completion is not claimed for this version.

The helper reports the commit, assembly version, XLL hash and an isolated test folder.
It neither installs the AddIn nor changes version/tags. Dependency-manifest checks
are necessary but do not prove that Excel can load every packed assembly.

## Excel gate

1. Close Excel and disable the previously installed AddIn. Avoid loading both copies.
2. Open the reported isolated XLL in 64-bit Excel. The directory contains only the XLL;
   do not copy supporting DLLs or layouts beside it.
3. Open Settings; verify the displayed assembly version is 1.2.0.
4. Create workbooks with known AJ/GL/AF samples for each supported producer and
   relevant format (CSV/XML/XLS/XLSX/PDF/XPS/OXPS/DBF). Check rows, totals, identity,
   source date warnings, reconciliation and generated worksheets against references.
5. Check Softip monthly selection, duplicate/mixed-year rejection, malformed CSV,
   multi-worksheet rejection, invalid controls and unknown formats.
6. Identical IVES/IfoSoft CSV signatures leave software unidentified. Select the known
   source software explicitly; no automatic legacy fallback is allowed.
7. Record commit/hash, samples used and outcomes. Keep the last released XLL available
   for rollback. Verify the built V1.2.0 XLL before distribution.

## Remaining gates

Validation update, 2026-10-07:

- 255 SimpleTests cases passed with the Linux direct-compiler test harness. This is
  not a Windows MSBuild or standard VSTest run, and does not validate external corpora.
- The materiality writer compiled against the Excel interop assembly. The supplied
  Excel screenshot confirms current equity 54,949.31, previous equity 59,534.43,
  and performance materiality 549.49 with the template settings.
- Explicit add-in package references are covered by the packing manifest, excluding
  Excel-DNA and Office interop references supplied by the host.
- Windows release tests, the full add-in rebuild, and an isolated packed-XLL smoke
  test remain required. Run Test-ReleaseCandidate.ps1 and retain its output/TRX files.
- In Excel, verify that _WB_significance selects the PerformanceMat result cell,
  changes to source/percentage flow through to account-detail processing, and saving
  and reopening preserves the formulas and hidden metadata. Confirm Navigation has
  no duplicate significance input and headings use Center Across Selection.

Build V1.2.0 on Windows, verify the packed XLL in Excel, and retain the package hash.
Tag the exact source commit after that build is confirmed.

## Regression follow-up after Windows run at 661790d

The Windows run passed 255 SimpleTests and failed 14 of 254 accounting tests;
therefore the Release rebuild and packed-XLL checks did not run.

The follow-up fixes metadata fallback ordering, source detection with letter-suffixed
filenames, and the missing inline 28-column IVES GL layout. The synthetic Urbis
fixture now includes its printed total; the staged Softip fixture includes identifying
headers and a synthetic total. Standalone footer page labels are validated and ignored
as data. The duplicate regression-manifest entry is removed and Medzilaborce settings
are supplied from the paths already present in that manifest.

The uploaded Dúbravka sample (SHA256
3a3efb7193d13a6eb4a29dd37fdf795588550b20194e21d64c0c6d369ac913fe)
is valid CSV: 714 rows, each with 20 fields. Both legacy and staged imports pass
metadata, row, and balance assertions. The external regression now checks this
corrected file; the separate synthetic unterminated-quote diagnostic test remains.
The old MALFORMED environment-variable name is retained as an alias.

Windows follow-up at 6fba9c2 passed 257 SimpleTests and 254 of 256 accounting tests.
Besides the obsolete Dúbravka assertion, the corpus gate reports two missing files
under `00323292 - Obec Nižná Jablonka`: `HL_KNIHA_00323292_2024.pdf` and
`U_DENNIK_00323292_2024.pdf`. The other 78 corpus files parsed. Restore the files
or verify their correct manifest paths; do not silently skip them.

Local follow-up validation: 257 SimpleTests and 52 focused accounting regression
cases passed in the direct-compiler harness. Windows and external-corpus rerun pending.

## V1.2.0 preparation decision — 2026-10-07

The user authorized preparing V1.2.0 and deferring the larger corpus. The two missing
Nižná Jablonka PDF entries were removed from the old manifest; its 78 remaining
entries are retained. The two existing ledger variants remain covered. This is a
manifest scope change, not a claim that the missing files passed.

Build the Windows package from a clean checkout:

```powershell
.\deployment\ExcelApiPoc.AddIn\Build-Package.ps1 -Version 1.2.0
```

Windows package creation and the V1.2.0 packed-XLL smoke test remain unverified here.

## Multiple journals — Sobrance URBIS, 2026-10-07

The five original XLS journals supplied by the user each contain one worksheet and
individual printed document/report totals. Combined import yields 29,582 records,
with debit and credit both 36,628,672.90. Against the supplied XLS general ledger
(556 accounts), both turnover differences are zero. No merged workbook or relaxed
printed-total validation is used. 262 SimpleTests passed locally, including combined
journal provenance, duplicate-content rejection and invalid-part rejection.
The updated Windows add-in must be rebuilt before use; the previous V1.2.0 package
will not include multi-file support.
