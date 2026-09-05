# Calculation-report candidate V1

This database-only migration adds an inline table-valued function that returns
mapped RegisterUZ financial-report candidates for one IČO and full calendar
fiscal year. It does not change the API or Excel add-in.

Prerequisite:

- `Migrations/CalculationTemplateV1` has been applied and validated.

Apply:

1. `010_CreateCalculationReportCandidateFunction.sql`
2. `020_ValidateCalculationReportCandidateFunction.sql`

The function applies these rules:

- exact January-through-December fiscal period;
- ordinary statement (`Riadna`);
- public report (`Verejné`);
- current, non-deleted entity, statement and report records;
- at least one materialized financial-report table;
- the RegisterUZ template must exist in
  `Accounts.CalculationTemplate`.

It returns every match and never uses `TOP (1)`. The later API layer must
interpret cardinality explicitly:

- zero rows: no mapped calculation report was found;
- one row with `CalculationImplemented = 0`: recognized but not implemented;
- one row with `CalculationImplemented = 1`: eligible for package resolution;
- multiple rows: ambiguous configuration/report selection.

`LegalFormCode` is returned for optional diagnostics only and is never used as
a selector.

The validation performs a known template-690 probe for IČO 00322792 and fiscal
year 2024 when that entity is loaded in RegisterUZ.
