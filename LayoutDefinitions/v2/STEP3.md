# Step 3 — common engine and format adapters

Baseline: `a6bb731fbc14f0eae0b1573fdda9abd03be4759e`, branch `feature/ifosoft-oxps-layouts`.

This is an opt-in review implementation. It executes the migrated v2 definitions and emits the v2 result contract. It does not replace the existing import routing or enable production exports.

## Included

- `PdfLayoutEngine.V2`: immutable catalog snapshots; JSON Schema, hash and reference validation; recognition; per-page ruling/label calibration; independent block candidates; glyph field assignment; explicit parsers; metadata and fiscal-year derivation; classification; local groups and closing-footer inheritance; completeness and record balance equations; evidence and reversible transforms.
- `PdfLayoutEngine.IText.V2.ITextPositionedAdapter`: PDF glyphs and stroked straight rulings, crop origin, page rotation, UserUnit, clipping diagnostics.
- `PdfLayoutEngine.Xps.XpsPositionedAdapter`: separate, platform-independent XPS/OXPS adapter. Resolves package relationships and document/page references in declared order; decodes embedded font advances and TrueType bounds; handles glyph indices/clusters, nested affine transforms, straight path rulings and rectangular clipping.
- `PdfLayoutEngine.Tests/V2Tests.cs`: portable tests for the new pipeline components and adapters.
- Definition revision `2.0.0-draft.5`: capped layout-marker support and literal GMX title punctuation, in addition to the previous title signals. Geometry remains mandatory. The migration generator reproduces these changes. No source geometry was changed.

## Use from C#

Reference `PdfLayoutEngine`, `PdfLayoutEngine.IText` and `PdfLayoutEngine.Xps`.

```csharp
using PdfLayoutEngine.V2;
using PdfLayoutEngine.IText.V2;
using PdfLayoutEngine.Xps;

var catalog = DefinitionCatalog.Load("LayoutDefinitions/v2", allowReviewDrafts: true);
var engine = new PositionedLayoutEngine(catalog);
IPositionedDocumentAdapter adapter =
    Path.GetExtension(inputPath).Equals(".pdf", StringComparison.OrdinalIgnoreCase)
        ? new ITextPositionedAdapter()
        : new XpsPositionedAdapter();

var result = engine.Process(inputPath, adapter,
    new EngineOptions { Extraction = "records", Validation = "internalConsistency" },
    new ExtractionLimits(), cancellationToken);
File.WriteAllText(outputPath, result.ToJson(), new UTF8Encoding(false));
```

`Extraction`: `recognition`, `metadata`, `records`.
`Validation`: `none`, `completeness`, `internalConsistency`.
Validation promotes the effective extraction level to records; requested and effective levels are both reported. Adapters and execution accept cancellation and resource budgets. Invalid packages/catalogs and exhausted budgets throw; callers must report those failures rather than interpreting them as empty successful imports.

Reuse the catalog between documents. Process each document with an explicitly chosen adapter. There is no filename-based entity, year or layout inference. Definition hashes normalize CRLF to LF; source document hashes use the exact bytes.

Results include structural occurrences (headers, footers, subtotals) as well as detail rows. `records.Count` is therefore not the number of exportable accounting rows. Inspect `semanticClass`, `exportEligibility`, field states, validation and processing status together.

## Review boundaries

- These definitions are still `reviewDraft` and `productionApproved: false`. Loading them requires explicit opt-in. Existing v1 entry points remain intact.
- Missing/ambiguous metadata remains unresolved. In particular, there is no verified reporting-entity CIN locator; licensed-user text is not substituted for the entity. Two-digit years require printed period context.
- Original-input completeness remains unknown unless partial-input evidence establishes a gap. Matching totals and contiguous page labels never establish original completeness.
- Group membership is retained locally. Cross-page groups without explicit continuity evidence are split and diagnosed; inheritance never crosses those boundaries. Parent-group reconciliation and cross-page continuity semantics still need fixture-backed rules.
- Aggregate/control reconciliation remains disabled as specified by Step 2. The enabled record-level balance equations execute with decimals. Blank operands are not zero.
- Equivalent or competing block/variant interpretations remain ambiguous. Some real records consequently need review. The detector uses provisional tolerances; this is not a corpus-validated extractor.
- Unsupported XPS features (including sideways/RTL glyphs, resource-based transforms, property-element clipping/path geometry, opacity masks and non-straight paths) are diagnosed or rejected. They never silently produce a clean result. PDF complex clipping is conservatively bounded and disables clean success. Scanned documents have no OCR path.
- C# business-model projection is not enabled: the definition models still declare `domainMappingStatus: pendingReview`. The legacy domain models have non-nullable amount/date properties that cannot safely represent all unresolved v2 states. The v2 result preserves those states rather than filling legacy properties with zero/default dates. Production domain mappings require a separate reviewed change.
- The engine implements the operations used by the migrated, enabled rules. This is not a promise that every future schema-valid rule combination is executable; unsupported operations reject explicitly. Disabled aggregate operations remain not evaluated.

## Verification

```powershell
python -m pip install -r LayoutDefinitions/v2/tools/requirements.txt
python LayoutDefinitions/v2/tools/validate.py --self-test
dotnet test PdfLayoutEngine.Tests/PdfLayoutEngine.Tests.csproj --filter "FullyQualifiedName!~DiagnosticTests"
```

The filtered test run excludes existing environment-dependent diagnostic tests, which require separate sample directories. The full test run in this workspace reported 92 passes and 28 missing-fixture failures. The portable run passed all 92 tests, including 21 new v2 cases.

The definition self-test passes: 43 definitions, 11 layouts, 1,126 preserved geometry items, all 12 negative cases. That Python command validates definitions; it does not execute the C# engine.

Real PDF, XPS and OXPS adapters were exercised. An 18-page account-plan XPS was processed end to end. Ledger PDF and journal OXPS were decoded completely and their first two pages processed for bounded integration checks. All results remain review/incomplete; these checks do not establish record-level accounting correctness or full-corpus coverage. See `sample-checks.json` in the delivery package for observed counts and scope.

Step 4 (CSV manifest runner, corpus-wide summaries and expectation comparisons) is not included.
