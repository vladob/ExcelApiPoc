INVENTARIZACIA - Account Balance Verification 0.1.12 (auditor test release)

REQUIREMENTS AND INSTALLATION
Windows, Microsoft 365 Excel 64-bit, .NET Framework 4.8.
1. Extract the entire ZIP to a local folder.
2. Close all Excel windows.
3. Run Install.cmd, then start Excel and open the Inventarizacia ribbon.
No administrator rights are required. Keep the complete extracted package
until installation finishes. If organizational policy blocks unsigned add-ins,
ask your administrator to approve this package.

CREATE A WORKBOOK
Choose Vytvorit inventarizacny zosit.
MkSoft: configure the API URL and your own API key in Nastavenia, then select
client/year, load details and create. Office network/VPN may be required.
Subory: select a journal and/or general ledger, optionally a chart of accounts.
Review the accounting system, ICO, entity name and fiscal year before creating.
File import does not require the API. No API keys or user settings are packaged.

PARAMETRE
Editable fields are highlighted near the top, including entity identity,
address and dates. Public parameter names start with P_. MkSoft address
retrieval requires the updated server API and database wrappers.
Enter people in ABV_People. Prva and Ucty control title/account signatures.
For Menovaci dekret select Prijemca or Podpisujuci; for Rekapitulacia select
Komisia or Podpisujuci. Blank names are omitted. Extend the table for more people.
Documents recalculate with Excel formulas; no refresh button is needed.
Use Microsoft 365 supporting FILTER, LET and SEQUENCE. In manual calculation
mode press F9. Keep dynamic-array spill areas clear.

PRINT AND PDF
Select the document in the Tlac a export dropdown, then use Tlacit or
Exportovat do PDF:
- Inventarizacia: title page and account worksheets, one PDF.
- Rekapitulacia: only the recapitulation worksheet, one PDF.
- Menovacie dekrety: decree pages only; PDF export creates one file per
  recipient in a new Menovacie_dekrety_... subfolder of the selected folder.
All three PDF options share a remembered destination folder across sessions.
For decrees the remembered location is the parent, not the generated subfolder.
If normal PDF export fails, close the destination PDF in its viewer and retry,
or choose another filename. Cancel ends the export without closing your workbook.
Buttons follow the active workbook and selected document. Finish a cell edit
with Enter or cancel it with Esc before running a command.
Use ribbon output for the defined sheet selection and adjusted print areas.
Ordinary Excel print does not apply the add-in's selection rules.

REKAPITULACIA PROTOTYPE
This is a municipality-oriented prototype for auditor review, not a completed
universal report template. Verify account mappings, source completeness,
classifications and amounts before signing. Conclusions require auditor input.
Zdroj rekapitulacie contains imported ledger data, including classes 5-7.
Mapovanie rekapitulacie exposes mapping patterns, signs and manual overrides.
These support sheets and Parametre are excluded from ribbon output.
Special analytical mappings are prefilled only for the Kamienka 2025 example;
other entities/years require those mappings to be completed. Missing mappings
remain blank. Manual numeric zero is a valid override; overrides apply per
mapping row, so do not repeat a whole-item override across all its patterns.
Income minus expenses assumes pre-closing class 5/6 balances. Loan maturity
and other classifications may require manual review/additional evidence.
Section conclusion rows are unmerged with Wrap Text off. Insert whole worksheet
rows for additional comment lines. Other report narrative cells may be merged.
The final signature line has a 45-point row height. Check pagination after edits.
The prototype supports up to 20 named commission members.

TEST CHECKLIST
- Create and save a workbook; verify entity/year, imported balances and address.
- Edit parameters and people; verify title, account, decree and recap formulas.
- Fill conclusions and verify mapping/override results.
- Preview/export each of the three document selections independently.
- Re-export inventory/recap with the destination PDF open; test retry/cancel.
- Check long comments, signatures and page boundaries in the actual PDF.
- Check buttons when switching workbooks and entering/leaving cell edit mode.
- Check the shared export folder, including after restarting Excel.
For any issue, send the error text, steps, selected source/year and a screenshot
or sample workbook as appropriate.

UNINSTALL AND ADMINISTRATION
Close Excel and run Uninstall.cmd. Workbooks and user settings are retained.
This installer does not change the separate ExcelApiPoc.AddIn registration.
This ZIP installs only the Excel add-in, not the API or SQL database.
Deploy the compatible API and 040_RecapAddress.sql separately for MkSoft use.
Installed files: %LOCALAPPDATA%\AccountBalanceVerification\AddIn\0.1.12
User configuration: %APPDATA%\AccountBalanceVerification
