Step 3c changes
================

Changed files:
- ExcelApiPoc.Api/Data/RegisterUzAccountingEntityRepository.cs
- ExcelApiPoc.Api/Program.cs

What changed:
- Loads FinancialReportTable rows for the requested RegisterUZ entity.
- Loads sparse FinancialReportValue rows and attaches them to their table.
- Preserves explicit zero values because every stored sparse row is returned.
- Adds debug counts for tables, sparse values, and explicit zero values.
- Removes the unused `using System.Net.Mail;`.

No DTO changes are required for Step 3c.
