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

The integration was prepared in Linux; .NET/MSBuild failed during startup due to
unavailable process information, before compilation. No build or test pass is claimed.
Windows tests and packed-XLL execution remain pending. Existing coordinator tests may
expose compatibility differences between old and staged importers; investigate failures
rather than disabling the tests. After all gates pass, bump to 1.2.0, rebuild, repeat
the packed-XLL smoke test, then tag the exact verified release commit.
