# Version 1.2.0 validation (assembly remains 1.1.4.0)

This branch incorporates preserved WIP commit fd33453 and connects the AddIn to
CompactLayoutImporter through StagedImportRuntime. The two historical lenient-CSV
commits are not cherry-picked; their examples are covered at the runtime boundary.
The source assembly version and Settings display are unchanged. No release tag exists
for these changes.

## Windows automated gate

From the repository root, using PowerShell with Visual Studio Build Tools and .NET 8:

```powershell
.\deployment\ExcelApiPoc.AddIn\Test-ReleaseCandidate.ps1
```

The script loads LocalTests.runsettings by default for both suites. Use
-RunSettingsPath to select another settings file; its external sample paths must exist.
The script stops on failed tests/build/dependency checks. Both staged and accounting
regression suites must pass. Corpus tests that return early without their environment
variables are NOT evidence that the external corpus passed. Run the existing corpus
runner separately with the actual input manifest before approving release.

The helper reports the commit, assembly version, XLL hash and an isolated test folder.
It neither installs the AddIn nor changes version/tags. Dependency-manifest checks
are necessary but do not prove that Excel can load every packed assembly.

## Excel gate

1. Close Excel and disable the previously installed AddIn. Avoid loading both copies.
2. Open the reported isolated XLL in 64-bit Excel. The directory contains only the XLL;
   do not copy supporting DLLs or layouts beside it.
3. Open Settings; verify the displayed assembly version is 1.1.4.
4. Create workbooks with known AJ/GL/AF samples for each supported producer and
   relevant format (CSV/XML/XLS/XLSX/PDF/XPS/OXPS/DBF). Check rows, totals, identity,
   source date warnings, reconciliation and generated worksheets against references.
5. Check Softip monthly selection, duplicate/mixed-year rejection, malformed CSV,
   multi-worksheet rejection, invalid controls and unknown formats.
6. Identical IVES/IfoSoft CSV signatures leave software unidentified. Select the known
   source software explicitly; no automatic legacy fallback is allowed.
7. Record commit/hash, samples used and outcomes. Keep the last released XLL available
   for rollback. Do not distribute this validation build as 1.2.0.

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

After all gates pass, bump to 1.2.0, rebuild, repeat the packed-XLL smoke test, then
package and tag the exact verified release commit. Assembly remains 1.1.4.0 until then.

## Regression follow-up after Windows run at 661790d

The Windows run passed 255 SimpleTests and failed 14 of 254 accounting tests;
therefore the Release rebuild and packed-XLL checks did not run.

The follow-up fixes metadata fallback ordering, source detection with letter-suffixed
filenames, and the missing inline 28-column IVES GL layout. The synthetic Urbis
fixture now includes its printed total; the staged Softip fixture includes identifying
headers and a synthetic total. Standalone footer page labels are validated and ignored
as data. The duplicate regression-manifest entry is removed and Medzilaborce settings
are supplied from the paths already present in that manifest.

The external Dúbravka malformed-CSV test remains unchanged: the tested local file
was accepted, whereas this test expects an unterminated quote at line 13. A new
self-contained test verifies that exact parser diagnostic. Inspect the external file
and its environment-variable path before deciding whether the sample was corrected.
Do not disable the test or claim the external corpus passed without rerunning it.

Local follow-up validation: 257 SimpleTests and 52 focused accounting regression
cases passed in the direct-compiler harness. Windows and external-corpus rerun pending.
