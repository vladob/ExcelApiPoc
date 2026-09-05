# ExcelApiPoc TODO

This file is the working project roadmap. Product code, database objects and API
contracts continue to use full domain names rather than informal abbreviations.

## 0. Record the AuditAddIn V1.0 database milestone

- [x] Back up and verify the legacy `AuditAddIn` database.
- [x] Back up and verify the accepted normalized `AuditAddIn_New` database.
- [x] Remove the two temporary live databases.
- [x] Recreate `AuditAddIn` entirely from the canonical scripts.
- [x] Populate scripts 010 through 080 successfully.
- [x] Run script 090 and complete structural and data validation successfully.
- [x] Use clean logical database file names: `AuditAddIn` and
      `AuditAddIn_log`.
- [x] Run the API under `IIS APPPOOL\ExcelApiPoc.Api` using Windows integrated
      security.
- [x] Grant the IIS identity read-only access to `AuditAddIn`.
- [x] Grant the IIS identity the required read/write access to `RegisterUZ`.
- [x] Verify API and add-in operation with an existing RegisterUZ entity.
- [x] Verify on-demand RegisterUZ loading with a previously uncached IČO.
- [x] Create and verify the checksum-protected, compressed, copy-only
      `AuditAddIn` V1.0 backup.
- [ ] Add the final scripts 000 through 090 to the repository.
- [ ] Archive the old 095 comparison scripts; do not include them in the normal
      rebuild sequence.
- [ ] Record the V1.0 backup filename, date, Git commit and validation result.
- [ ] Commit and push the reproducible AuditAddIn V1.0 database milestone.

## 1. Remove hardcoded accounting-framework selection

### Agreed selection rule

The audited fiscal year's RegisterUZ financial-report template is authoritative.
`LegalFormCode` must not select the accounting framework. The same accounting
entity and legal form may use different report templates in different periods,
for example `699 - Úč POD` and `687 - Úč MUJ`.

Resolve the calculation package through the existing normalized relationship:

```text
RegisterUZ.Templates.FinancialReportTemplate.RegisterUzTemplateId
    -> AuditAddIn.Template.Templates.ErpId
    -> AuditAddIn.Accounts.TemplateFrameworkVersion
    -> AccountFrameworkVersion
    -> AccountFramework
    -> CalculationConfigurationVersion
```

- [x] Select the relevant financial statement and report for the requested IČO
      and audited fiscal period.
- [x] Do not identify calculation-capable reports from names such as
      `Oznámenie...`; determine support from AuditAddIn configuration.
- [x] Add an API database contract that accepts `RegisterUzTemplateId` and an
      applicable date or fiscal year.
- [x] Resolve exactly one applicable `TemplateFrameworkVersion`.
- [x] Return the framework code, framework-version identity and
      calculation-configuration identity with the calculation package.
- [x] Return a clear unsupported result when no applicable calculation
      configuration exists.
- [x] Treat multiple applicable configurations as a configuration error; never
      guess.
- [x] Remove the hardcoded `GOV_LOCAL` argument from
      `CreateAuditWorkbookForm`.
- [x] Use `LegalFormCode` only as optional validation or a fallback hint.
- [ ] When no filed report exists, allow explicit selection of a supported
      report template or limited import-only operation; do not infer a template
      silently.
- [ ] Add tests for an entity whose legal form stays unchanged while its report
      template changes across fiscal years.
- [ ] Add tests ensuring approval notices and other supporting templates are not
      selected for calculation.

## 2. Establish database query contracts with table-valued functions

- [ ] Inventory API SQL queries suitable for inline table-valued functions.
- [ ] Keep each TVF limited to one logical result set.
- [ ] Pass framework, template, entity and applicable-date parameters
      explicitly.
- [ ] Return stable identifiers, ordinals and sort-order columns.
- [ ] Keep final `ORDER BY` clauses in callers because a TVF does not guarantee
      result order.
- [ ] Implement AuditAddIn TVFs one API operation at a time.
- [ ] Prove equivalence with counts, bidirectional `EXCEPT` and API JSON
      comparisons.
- [ ] Benchmark RegisterUZ candidates before adopting them.
- [ ] Keep CRUD operations in API-controlled parameterized commands or stored
      procedures rather than TVFs.

Initial candidates:

- applicable account framework and accounts;
- account ranges;
- template metadata and layout;
- applicable calculation configuration;
- account calculation rules;
- report-account mappings;
- calculation plan;
- RegisterUZ entity reports;
- RegisterUZ financial-report values.

## 3. Add client-to-API authentication

Database authentication is already handled separately through the IIS
application-pool identity. This section concerns clients authenticating to the
HTTP API.

- [ ] Add stable `ApiPrincipal` identities.
- [ ] Add one-to-many administrator-managed `ApiKey` credentials.
- [ ] Create initial principals as required for Google, Azure and Doklado
      integrations.
- [ ] Do not create keys automatically.
- [ ] Do not impose automatic expiration in the first phase.
- [ ] Generate keys cryptographically and display the full value only once.
- [ ] Store only key hashes plus a safe prefix or fingerprint.
- [ ] Send keys in a request header, never in a query string.
- [ ] Ensure complete keys are never logged.
- [ ] Support explicit activation, disabling, revocation and multiple keys per
      principal for controlled rotation.
- [ ] Record `LastUsedAtUtc` without turning it into an automatic limitation.
- [ ] Add secure API-key configuration to the Excel add-in.
- [ ] Provide clear unauthorized and forbidden responses.
- [ ] Plan a compatibility rollout so existing installed add-ins are not broken.

## 4. Implement predefined auditor comments end to end

Comments belong to an `ApiPrincipal`, not directly to an API key. Rotating a
credential must not transfer or orphan the auditor's comments.

- [ ] Design the predefined-comment table and ownership relationship.
- [ ] Include title, text, category, sort order, active state and timestamps.
- [ ] Add a `rowversion` concurrency token.
- [ ] Decide whether deletion is physical or represented by an inactive state.
- [ ] Decide how administrators manage comments belonging to other principals.
- [ ] Add authenticated list/search, create, update and delete API operations.
- [ ] Record the authenticated principal for every mutation.
- [ ] Add audit logging for comment mutations.
- [ ] Create an Excel workbook for comment administration.
- [ ] Add convenient comment selection and insertion to the actual audit
      workflow.

## 5. Generalize multi-year financial-report comparison

The audited fiscal year's report template is the reference template.

- [ ] Add explicit report-template compatibility groups.
- [ ] Add explicit comparable-row mappings when compatible templates do not
      share stable row identities.
- [x] Include only fiscal years whose templates are explicitly compatible with
      the audited fiscal year's template.
- [ ] Never infer compatibility from similar names, row numbers or table shape.
- [ ] Allow missing years without treating them as errors.
- [ ] Omit incompatible years rather than manufacture comparability.
- [ ] Preserve and expose `IsSum` plus summarization metadata.
- [ ] Generalize the existing template-690 implementation without adding another
      template-specific branch.
- [ ] Configure template 690 and its compatible predecessors/successors as the
      first compatibility group.
- [ ] Evaluate template 727, including tables `Výnosy` and `Náklady`, as the
      next comparison family.

This comparison remains a useful secondary feature, not a blocker for the core
audit-calculation workflow.

## 6. Add the Urbis XLS importer family

- [ ] Collect representative Excel 97 `.xls` files for AF, GL and AJ.
- [ ] Document worksheet-name, header-row and column-order variability.
- [ ] Document cell types, date/decimal formats, formulas, stored values,
      blank-row behaviour and merged cells.
- [ ] Read `.xls` directly without requiring installed Excel or COM automation.
- [ ] Implement `UrbisXlsAccountingFrameworkImporter`.
- [ ] Implement `UrbisXlsGeneralLedgerImporter`.
- [ ] Implement `UrbisXlsAccountingJournalImporter`.
- [ ] Normalize all three into the same contracts consumed by workbook
      creation.
- [ ] Add representative fixtures and importer tests.

## 7. Strengthen validation and testing

- [ ] Add an API integration-test project.
- [ ] Cover framework/template resolution for supported, unsupported and
      ambiguous configurations.
- [ ] Validate that framework and calculation-configuration validity periods do
      not overlap.
- [ ] Validate cross-framework foreign-key consistency.
- [ ] Add automated database drift checks for frameworks, configurations,
      mappings and calculation plans.
- [ ] Add regression tests for template 690 calculations and reconciliation.
- [ ] Add tests for RegisterUZ on-demand loading under the deployed identity.

## 8. Controlled reference-data maintenance

- [ ] Design controlled synchronization from `OfficialAccounts` into the
      reviewed production `Accounts` snapshot.
- [ ] Produce an explicit change report before applying additions, removals or
      renamed accounts.
- [ ] Require review before a new framework version becomes calculation-ready.
- [ ] Replace placeholder source/provenance metadata such as `Book1`,
      `Book2` and `scan` with durable references when available.
- [ ] Resolve or document cases where `VersionCode` differs from `ValidFrom`.

## 9. Release, operations and repository guidance

- [ ] Add repository-level `AGENTS.md` with SQL style, safety rules, build
      commands, verification steps and Git conventions.
- [ ] Document the database rebuild and validation sequence.
- [ ] Document IIS application-pool identity provisioning for AuditAddIn and
      RegisterUZ.
- [ ] Remove personal SQL credentials from the standalone RegisterUZ loader and
      assign an appropriately scoped service identity.
- [ ] Define add-in/API/database compatibility and release-version rules.
- [ ] Define the deployment sequence for framework-aware add-in versions.

## Later: analytical publication

- [ ] Design the OLAP or semantic publication model.
- [ ] Publish report facts with entity, period, template, table, row, framework
      and version identities.
- [ ] Publish all report rows together with `IsSum` and summarization paths.
- [ ] Make double-counting avoidance explicit for Excel and other consumers.

## Informal working shorthand

These abbreviations are only a conversation and planning cheat sheet. They are
not product terminology and should not replace full names in code, SQL objects,
API contracts or formal documentation.

| Abbreviation | Meaning |
|---|---|
| AE | Accounting Entity |
| AF | Accounting Framework |
| AJ | Accounting Journal |
| GL | General Ledger |
| CR | Calculated Report |
| FR | Financial Report from RegisterUZ |
| FS | Financial Statement |
| AR | Annual Report |
| RT | Report Template |
| CC | Calculation Configuration |
| RM | Report Mapping |
| RUZ | RegisterUZ |
