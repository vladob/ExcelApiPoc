# Positioned-document definitions v2 — Step 2

Version: `2.0.0-draft.4`. Based on the approved `layout-structure.v2.draft3.json`.

This package supplies formal JSON Schemas, typed IfoSoft definitions, reproducible
migration tools and offline validation. It is **not loaded by the current C#
engine**. All definitions remain `reviewDraft`, `productionApproved: false`.
`enabled` denotes a proposed rule to execute once Step 3 implements its operation;
it does not mean that the current engine supports it.

## Contents

| Directory/file | Purpose |
| --- | --- |
| `schemas/` | 2020-12 schemas; closed executable objects and operation-specific arguments |
| `catalog.json` | Exact-version references, SHA-256 hashes, coordinate convention and limits |
| `identification/` | Separate category, producer and layout scores; capped correlated evidence |
| `layouts/` | Geometry, field parsers, concrete metadata locators, classification, groups and completeness |
| `parsers/` | Identifier, decimal, integer, exact date/time and accounting-period parsing |
| `models/` | Typed source semantic fields; domain export mappings await Step 3 review |
| `validation/` | Explicit operands and scopes; concrete printed-control bindings |
| `sources/` | Approved proposal, original review archive and account-plan GMX sources |
| `tools/` | Deterministic authoring/migration and offline validation |
| `verification.json` | Recorded schema/reference/preservation checks; not an extraction test report |

11 layout definitions, 13 variants, 9 identification families, 11 models,
11 validation profiles, one parser profile document: **43 catalogued definitions**.
The account-plan variants replace the empirical variant. DENNIK1 preserves its
original and alternate header. The separate legacy HLKNIA4 and simplified DENNIK
remain distinct layout candidates. PREDVAS3 is GL. OBR_UCET retains source account
movements while its AJ/GL domain category remains explicitly unresolved.

## Validate or rebuild

Python 3.10+ is sufficient for the authoring tools. Install the validator in an
appropriate Python environment:

```powershell
python -m pip install -r LayoutDefinitions/v2/tools/requirements.txt
python LayoutDefinitions/v2/tools/validate.py --self-test
```

To regenerate after editing the authoritative Python authoring sources:

```powershell
python LayoutDefinitions/v2/tools/build_schemas.py
python LayoutDefinitions/v2/tools/migrate.py
python LayoutDefinitions/v2/tools/validate.py --self-test
```

Generated JSON is the distribution interface. Regeneration overwrites files in
this v2 package; it never edits `PdfLayouts/` or a C# project. Schema `$id` values
under `schemas.excelapipoc.local` are logical identifiers, not hosted URLs. The
validator resolves them offline. It checks the JSON Schema metaschema, catalog
hashes, exact definition versions and kinds, reference targets, IDs in scope,
parser/model fields, group/calculation cycles, ordered rectangles, anchors and
membership selectors. The self-test mutates valid definitions and verifies
rejection, including unsupported engine capabilities. `check(capabilities=...)`
provides the future loader capability gate; ordinary validation does not certify
engine compatibility.

The geometry regression checks 1,126 fields, labels and rulings against the
preserved draft-2 archive. It excludes the intentionally replaced empirical
account-plan geometry. The new account-plan fields come directly from the two
preserved GMX sources, converted once at authoring time using 72/254.

## Execution semantics for Step 3

* Resolve the catalog before execution. References contain an exact ID and
  version; directory order has no meaning. Reject unknown executable properties,
  unknown operations and unsupported capabilities, including disabled rules.
* Definition validity, evidence status, rule enablement and production approval
  are independent. A valid definition is not proof of correct extraction.
* Normalize PDF to upright crop-local points, lower-left origin and upward Y.
  Account for UserUnit, rotation, crop origin and transforms already applied by
  the extractor. XPS/OXPS adapters must honor package page references, units,
  nested transforms and clipping, and preserve source-to-normalized transforms.
* Ruling-endpoint anchors are actual line endpoints, not assumed glyph corners.
  Fit one positive uniform scale and separate X/Y translations per physical
  page; enforce inlier count, span, separation, scale and residual bounds within
  the hypothesis limit. Do not infer scale from paper dimensions or borrow a
  preceding page's calibration. Use header ruling topology to retain competing
  hypotheses; geometryFit must include observed labels as well as rulings.
* `nominalPage` uses page calibration. `blockLocal` X=0/Y=0 is the independently
  detected block origin; local Y is negative below it. Apply page scale once.
  Nominal advance supports detection but never manufactures records. Empty
  source templates have their detection/assignment operations disabled.
* Candidate signals are a union; parsing success is not a discovery condition.
  Regions are soft windows. Split runs only with adequate glyph geometry;
  retain overlapping assignments and clipped evidence. Missing glyph geometry
  needed to separate fields yields unsupportedCapability, not guessed values.
* Classify all rules on detected block occurrences. Block identity requires
  matching observed geometry/labels, not just an internal name. No match retains
  an unrecognized candidate; incompatible matches remain ambiguous. Required
  fields apply by semantic class. Journal entries require at least one account
  side; the opposite side is never fabricated.
* Selectors: each nonempty class/membership list matches any item in that list;
  different selector properties combine with AND. Empty lists impose no filter.
  `level: any` imposes no level restriction. Select exactly one aggregation level.
  Record identity is physical occurrence identity, not a business-key deduplication.
* A reportSection is the detected report section, not proof that the input is a
  complete original report. `afterPreviousClosure` opens at the next eligible
  member after the preceding closure within the parent; initial groups are
  potentially partial. A matching closing occurrence supplies the key and
  closes preceding members. Repeated page headers never reset groups. Carry
  groups across pages only with recorded continuity evidence. Inherit closing
  metadata only into absent fields, retain provenance and diagnose conflicts.
* Recognition uses a bounded first-three-page scan, then up to twenty available
  pages if unresolved. Return exact inspected coverage when the budget ends.
  Evidence-group caps apply separately to each target/candidate across all
  families: repeated headers or duplicate family evidence cannot multiply scores.
  Contradictions subtract weight; veto signals prohibit confirmation. Scores are
  rankings, not probabilities. Missing signals are neutral unless required.
  A layout requires its geometry signal and confirmed category/producer gates.
  Ties retain candidates. An unresolved semantic category cannot authorize domain
  export, even if an OBR_UCET physical layout is recognized.
* Metadata uses actual variant/block/field or calibrated label-region locators.
  Compare repeated observations; never overwrite a conflict. Period metadata is
  an accounting-period code plus year, not a calendar date. Derive fiscalYear
  only from matching years in both printed period endpoints. Do not use the
  expected CSV year to resolve a two-digit transaction year.
* Metadata-only requests may inspect later pages for declared footer locators.
  Full-document metadata coverage is distinct from recognition scan coverage.
* Parse identifiers without losing zeros, letters or internal spaces. Keep
  monetary arithmetic decimal; successful distinct parses remain ambiguous.
  Retain raw period codes beyond the observed 00–14 range with a diagnostic.
* Blank, invalid, missing, unresolved and ambiguous operands are not zero.
  Row GL equations are scoped to accountBalance records. Split-sided PREDVAS3
  compares calculated opening/closing nets while preserving each printed side.
  The literal zero in net-closing addition is an arithmetic identity, not a
  fallback for a missing value. Comparisons never cure parsing ambiguity.
* Printed aggregate controls and their calculations are explicitly disabled
  until fixtures establish scope, member eligibility and blank suppression.
  Partial groups/documents cannot pass controls requiring complete coverage.
* Validation may promote effective extraction: completeness/internalConsistency
  require records. Report requested and effective levels separately. Cache page
  decoding within a request; cache immutable definitions by exact version/hash.
  Enforce all catalog limits and return incomplete results with retained evidence
  on exhaustion. Never silently fall back to arbitrary operations.

The future C# loader must implement these semantic checks as well as JSON Schema
validation. The Python validator is authoring tooling, not the production loader.
No PDF/XPS/OXPS extraction was run as part of Step 2.

## CSV manifest and results (Step 4)

`manifest-row.schema.json` validates each decoded CSV row: UTF-8 with optional BOM,
semicolon delimiter and headers FilePath, AccountingEntity, PerceivedCategory,
PreviousTestResult, ExpectedYear. Parse AccountingEntity at the first ` - ` when
preceded by an eight-digit CIN; retain the complete source text and leading zeros.
Aliases: HL_KNIHA -> GL, U_DENNIK -> AJ, UCT_ROZVRH -> AF. PreviousTestResult `0`
means NotTested. ExpectedYear is four digits or blank. Relative paths resolve
against the manifest directory. Explicitly report excluded/unsupported formats.
User-confirmed corrections to rows 101/106 of the second supplied CSV are 2025;
this package does not edit the repository manifest or infer which CSV numbering
convention was used.

Never pass expectations into recognition, parsing or calibration. Compare after
independent processing. `request.schema.json` intentionally has no expected
entity/category/year properties. `result.schema.json` holds the full extraction
result; `manifest-result.schema.json` adds expectations and separate statuses.
Monetary result values serialize as decimal strings. Period value strings retain
raw code/year; rawText and evidence retain original formatting. Each named total
includes a scope-occurrence ID. One CSV summary row per manifest row should flatten:

`ManifestRowNumber;FilePath;ExpectedCategory;ActualCategory;ExpectedCIN;ActualCIN;ExpectedEntityName;ActualEntityName;ExpectedYear;ActualYear;ProcessingStatus;RecognitionStatus;ExpectationStatus;ValidationStatus;RecordCount;InspectedPages;Coverage;OpeningDebit;OpeningCredit;OpeningNet;TurnoverDebit;TurnoverCredit;ClosingDebit;ClosingCredit;ClosingNet;Debit;Credit;Amount;AccountCount;Explanation;DetailResultPath`

Populate standard totals only when their semantic scope is established; otherwise
leave blank and explain. Multiple periods/groups and detailed control differences
belong in the JSON result. A passing expectation comparison is not validation success.

## Remaining evidence work

Calibration/recognition thresholds remain provisional. UROZVRH_V2 has not been
verified against matching print samples. Count membership for account-plan heading
and blank numbered rows needs full-document evidence; its unnumbered V2 variant
must not inherit heading-inclusive numbered-row count semantics automatically.
There is no verified reporting-entity CIN field in the supplied GMX definitions.
It therefore remains unresolved until a sample establishes a safe locator. User
license text is never promoted to entity name. Domain C# field mappings are
pending review. IVES and SoftipMop v1 definitions are outside this IfoSoft migration.

## Cross-platform text and catalog hashes

Generated JSON uses UTF-8 and LF newlines. Catalog definition SHA-256 hashes
are calculated over UTF-8 file bytes after replacing CRLF with LF, on both
write and verification. No JSON reserialization, whitespace stripping or other
content normalization is performed. This tolerates Git Windows line-ending
conversion while still detecting content changes. Source-artifact hashes remain
raw-byte SHA-256 values. All JSON reads explicitly specify UTF-8; GMX source
reads explicitly specify Windows-1250.
