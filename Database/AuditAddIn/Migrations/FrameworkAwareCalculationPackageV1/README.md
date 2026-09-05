# Framework-aware calculation package V1

Apply after the AuditAddIn V1.0 baseline:

1. `010_CreateApplicableTemplateFrameworkVersionFunction.sql`
2. `020_ValidateApplicableTemplateFrameworkVersionFunction.sql`

The function deliberately returns every applicable candidate. API code must
require exactly one row and must not use `TOP (1)` or legal form as a selector.
