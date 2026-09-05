# AuditAddIn database

`AuditAddIn` is the ExcelApiPoc-owned database for accounting frameworks,
financial-report templates, calculation configuration, report mappings and
calculation plans.

## Baseline V1.0

The scripts in `Baseline/V1_0` recreate the accepted AuditAddIn V1.0 state from
an empty SQL Server instance. Run them in filename order:

1. `000_CreateAuditAddInDatabase.sql`
2. `010_PopulateAccountFrameworks.sql`
3. `020_PopulateOfficialAccounts.sql`
4. `030_PopulateAccounts.sql`
5. `040_PopulateTemplates.sql`
6. `050_PopulateTemplateHeaders.sql`
7. `051_PopulateTemplateRows.sql`
8. `060_PopulateCalculationConfiguration.sql`
9. `070_PopulateReportAccountMappings.sql`
10. `080_PopulateRowCalculations.sql`
11. `090_ValidateAuditAddInDatabase.sql`

Script 000 refuses to continue when `AuditAddIn` already exists. It also creates
and maps the local Windows login `IIS APPPOOL\ExcelApiPoc.Api`, granting it
read-only access to the `Accounts` and `Template` schemas. SQL Server and IIS
must be on the same machine for this local virtual identity to work.

The final validation script is non-mutating. The accepted V1.0 validation
includes these principal counts:

| Object | Rows |
|---|---:|
| Account frameworks | 3 |
| Framework versions | 3 |
| Official accounts | 645 |
| Production accounts | 645 |
| Account calculation rules | 315 |
| Report-account mappings | 267 |
| Templates | 245 |
| Tables | 167 |
| Headers | 1,856 |
| Rows | 6,502 |
| Direct row-calculation terms | 188 |
| Generated calculation plan for template 690 | 959 |

Warnings about placeholder provenance (`Book1`, `Book2`, `scan`) and a
`VersionCode` differing from `ValidFrom` are recorded metadata observations;
they did not invalidate the V1.0 baseline.

## Design decisions

- Framework versions are whole snapshots. Individual accounts do not carry
  independent validity dates.
- `Accounts.OfficialAccounts` preserves imported reference evidence;
  `Accounts.Accounts` is the reviewed production source.
- Account captions are stored once per production account.
- Calculation configuration is versioned independently from official account
  wording.
- `Accounts.TemplateFrameworkVersion` connects a report template to the exact
  framework version and calculation-configuration version that support it.
- Direct formulas are stored in `Template.RowCalculationTerms`.
- `Template.GetCalculationPlan` expands the formula graph into the leaf-level
  execution plan.
- `IsSumRow` and formula relationships preserve summarization semantics.

## Report-template and framework selection

The audited fiscal year's RegisterUZ financial-report template is authoritative.
Do not select an accounting framework from `LegalFormCode`, and do not hardcode
`GOV_LOCAL` in the add-in.

The supported calculation path is:

```text
RegisterUZ FinancialReportTemplate.RegisterUzTemplateId
    -> AuditAddIn Template.Templates.ErpId
    -> Accounts.TemplateFrameworkVersion
    -> AccountFrameworkVersion
    -> AccountFramework
    -> CalculationConfigurationVersion
```

The applicable fiscal date must resolve exactly one configuration. Missing
support produces an explicit unsupported result; multiple matches are a
configuration error.

## Future changes

The first post-baseline migration is
`Migrations/FrameworkAwareCalculationPackageV1`. Apply its scripts in filename
order before deploying the framework-aware API and add-in.

- Put schema and behavioural upgrades in `Migrations`.
- Put reviewed additions such as a new calculation plan or template mapping in
  `DataUpdates`.
- Put reusable non-mutating checks in `Validation`.
- Never edit an already deployed baseline to disguise a later database change.

The removed temporary-database comparison and synchronization scripts documented
the one-time V1.0 cutover. They are retained in Git history and are not part of
the executable baseline.
