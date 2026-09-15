# Template 699 calculation V1

This reviewed data update enables the Slovak podnikatelia balance sheet and
income statement (RegisterUZ template 699) for the PROFIT account framework.

Run the scripts against `AuditAddIn` in filename order:

1. `010_PopulateTemplate699Calculation.sql`
2. `015_CorrectTemplate699BalanceSides.sql`
3. `020_ValidateTemplate699Calculation.sql`

The population script is rerunnable. It creates the PROFIT calculation
configuration, associates it with template 699, inserts account rules,
template-scoped report mappings and direct row formulas, and enables template
699 only after all prerequisite data has been inserted.

The mapping is automatic only where a synthetic account has one unambiguous
statutory destination. Accounts occurring in multiple leaf rows are exposed to
the add-in's analytical-mapping workflow.

The formula graph and signs were independently reconciled against an official
balance sheet and income statement. No client identity, ledger data or report
values are stored by these scripts.
