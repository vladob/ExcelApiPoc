# Calculation-template mapping V1

This database-only migration introduces the reviewed list of individual
RegisterUZ financial-report templates intended for calculation. It does not
change the API or Excel add-in.

Apply after the AuditAddIn V1.0 baseline:

1. `010_CreateAndPopulateCalculationTemplate.sql`
2. `020_ValidateCalculationTemplate.sql`

## Meaning of the mapping

- No row: the template is outside the intended calculation scope.
- `CalculationImplemented = 0`: the template is recognized and calculation is
  planned, but it is not available to users.
- `CalculationImplemented = 1`: calculation for the complete template has
  been implemented, tested, deployed and accepted.

The flag is a reviewed production-readiness switch. Runtime code must still
require exactly one complete, applicable and internally consistent calculation
configuration.

The initial mapping contains ten individual-report templates. Consolidated
templates 9, 11 and 684 and the intentionally excluded special-sector reports
are not mapped. Only template 690 is initially enabled.
