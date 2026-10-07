# ExcelApiPoc.AddIn V1.2.0

- Integrates the staged, layout-based accounting import runtime and embedded layouts.
- Adds IVES Excel report layouts, including the inline general-ledger variant.
- Resolves entity/year metadata from content, then filename fallback, then manual context.
- Adds PerformanceMat with five materiality tables and hidden mapping/source evidence.
- Uses official RegisterUZ values for the audited and previous years, summing matched
  report rows and retaining #N/A for unavailable mappings.
- Points _WB_significance to the performance-materiality result; new Navigation sheets
  no longer duplicate that input. Existing workbooks retain their legacy input.
- Uses Center Across Selection instead of horizontally merged cells.

Assembly/file version: 1.2.0.0. Target: 64-bit Excel, .NET Framework 4.8.

Validation: 257 SimpleTests passed in Windows. The subsequent accounting failures
were an outdated Dúbravka sample expectation (corrected and checked locally) and two
missing corpus paths (removed by user decision). The larger corpus run is deferred.
Windows package build and final Excel verification are still required.
