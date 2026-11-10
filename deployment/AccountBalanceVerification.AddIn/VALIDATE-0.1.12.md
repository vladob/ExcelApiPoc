# Inventarizácia 0.1.12 — recapitulation prototype

This is a source update, not a tested XLL release. Excel and the .NET SDK were unavailable in the implementation environment. Run Build.ps1 and the Excel checks below before installing it on the auditor's PC.

## Changes

- Parametre: entity name, ICO, fiscal year, dates and address at the top; editable inputs have a pale background. Public workbook names start with P_. ABV_People remains a structured table. Checkboxes are created only inside its body.
- Public names: P_EntityName, P_ICO, P_FiscalYear, P_Location, P_DocumentDate, P_InventoryStartDate, P_InventoryEndDate, P_AeAddress, P_DecreeIssueDate, P_AppointmentEffectiveFrom, P_InventoryDate, P_OrderNumber, P_OrderIssueDate, P_OrderReceiptDate. Existing internal names and date aliases remain usable.
- MkSoft entity lookup returns EntityAddress from ULICA, PSC, OBEC. Blank components do not produce stray separators. The ledger wrapper includes classes 5–7; account worksheets remain limited to classes 0–4.
- Rekapitulácia is a formula-driven municipality prototype with two commission tables, final signer, editable conclusions, and explicit mapping/override support. It is last in normal ribbon print/PDF. Decrees remain separate.
- Zdroj rekapitulácie stores the full imported ledger; Mapovanie rekapitulácie exposes patterns, sign, balance side and manual overrides. These support sheets are excluded from ribbon print/PDF. Source ledger amounts are static; changing parameters, people or mappings recalculates the report. Importing new accounting data still requires a new workbook.
- No XML importer changes.

## Deployment order

1. Review and apply `Database/AuditAddIn/Migrations/MkSoftVerificationV1/040_RecapAddress.sql` on the database server. It alters only API wrapper TVFs, not synchronization loaders, source tables or queues. Review the IIS identity grants for your environment.
2. Run `deployment/AccountBalanceVerification.AddIn/Build.ps1` from PowerShell. This runs the test project and builds the API and add-in. Deploy the rebuilt API after SQL. File-only testing does not need SQL/API deployment.
3. Close Excel, replace/load the built 64-bit XLL with its supporting files, and create a **new workbook**. Existing workbooks are not automatically migrated to the new parameter layout or given a recapitulation, because they may lack class 5–7 source data.

## Kamienka 2025 test input

Use fiscal year **2025**, IČO **00323110**, `HL_KNIHA_00323110_202512.CSV` and optionally `UCT_ROZVRH_00323110_2025.CSV`. Leave the journal unselected: the supplied CSV/DBF journal contains 2026 entries. The 2025 XML was supporting reconciliation evidence; do not import it through Inventarizácia.

Independent decimal checks against the supplied GL passed:

| Item | Opening | Additions | Reductions | Closing |
|---|---:|---:|---:|---:|
| 752 (sample-specific analytical accounts) | 6,119.50 | 304.80 | 1,112.98 | 5,311.32 |
| 08 | 486,833.29 | 54,745.78 | 10,087.88 | 531,491.19 |

Income minus expenses: **−18,083.00**. Listed receivables including 335: **5,033.81**.

## Excel checks still required

1. Confirm all top input cells have public P_ names, imported ICO retains leading zeros, and the MkSoft address appears when using the updated API. Change name/address/date: title, accounts and decree/report formulas should update without refresh.
2. Add chairman and members; select Komisia in the recap column. Select one final Podpisujúci. Confirm both commission tables update and no checkbox appears below ABV_People. Test adding a table row and changing a person's role/name.
3. Check that report numeric values match the table above. Compare the historical auditor figures in columns S:X of Mapovanie rekapitulácie. Historical values are only comparison constants; they never feed report calculations.
4. Change a mapping pattern or manual numeric override: the report and comparison should update. Blank unresolved patterns produce blank report results; a manual **0** is a real value. A nonnumeric override produces an error and blocks output. For keys with several patterns, overrides apply **per mapping row** and are summed; do not repeat the same whole-item override on every row.
5. Use both ribbon Print and PDF. Verify title → account sheets → Rekapitulácia; no support sheets or decree. Check page boundaries, wrapped names, numeric display, and conclusions for clipping. Unused reserved commission rows are hidden in the temporary output copy. Source workbook remains formula-driven.
6. Test separate decree print and individual PDFs; test existing 0.1.11 workbooks for regressions. Legacy preparation still handles the old layout.

## Provisional decisions for auditor review

- This prototype follows a municipality report, not a universal statutory template for all entity types. Section headings, selected accounts, inventory categories and analytical mappings require review. It does not establish completeness of the source import or cover every possible account automatically.
- Kamienka-specific bank, inventory and off-balance mappings are enabled only for IČO 00323110 / 2025. Other entities/years leave those mappings blank for the auditor to fill. Generic account groups are prefilled.
- Account 357 and other mixed settlement accounts use positive/negative analytical closing balances for asset/liability split. Confirm whether the auditor requires another classification.
- 461 is a combined loan balance. Current/noncurrent instalment classification needs additional information. Inventory and allowance categories also need confirmation.
- Income minus expenses uses pre-closing classes 5/6. If either class is absent, the result stays unresolved. If the source already contains closing entries, enter a separately verified result manually; automatic closing-status detection is not implemented.
- GL movements are accounting debit/credit movements normalized by account sign; these may differ from asset-register additions/disposals. The discrepancy with the auditor's 752 movements is deliberately visible.
- No-difference conclusions are not assumed. Yellow conclusion/approval fields must be completed by the auditor.
- Prototype commission capacity is 20 named members. Output checks prevent overflowing this capacity and multiple final signers. An unselected signer remains an explicit placeholder so draft review is possible.
- Direct Excel print is not the add-in output workflow; use ribbon Print/PDF to omit unused commission rows and order/exclude support sheets.

New RecapPlanTests cover isolation of entity/year-specific mappings, unresolved income results when source classes are missing, debit/credit splits and unchanged account-tab scope. They are included but have not been executed here.
