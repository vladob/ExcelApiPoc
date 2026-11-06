INVENTARIZACIA - Account Balance Verification 0.1.5 (pilot)

Requirements: Windows, Microsoft 365 Excel 64-bit, .NET Framework 4.8.
No administrator rights are required.

INSTALLATION
1. Extract the entire ZIP to a local folder (do not run inside the ZIP).
2. Close all Excel windows.
3. Run Install.cmd. Start Excel and use the Inventarizacia ribbon.
Do not change global Excel security settings. If your organization's policy
blocks unsigned add-ins, ask your administrator to approve this package.

CONNECTION
Obtain the API URL and your own API key from the administrator.
Use Nastavenia > Ulozit to save them and the include-empty-accounts option.
MkSoft: enter year and client ID or ICO, load details, review, then create.
Subory: select journal and/or ledger, optionally chart of accounts; review
identity/year and choose the accounting system if detection is ambiguous.
File imports use names from AF, then GL, and do not require an API connection.
MkSoft requires the API URL, your API key, MkSoft permission and synchronized
POSNOVA for the selected client/year. localhost means your own computer.
Connect to the office network/VPN as required.
No API keys or personal settings are included in this package.

PARAMETERS AND OUTPUT
Parametre contains location/date, inventory start/end dates, messages and the
people table. Choose a role, enter a name and select where signatures appear.
Blank names are omitted. Add more people by extending the Excel table.
Default roles can be changed in Nastavenia (one role per line).
Tlacit refreshes signatures and opens print preview. Exportovat do PDF refreshes
them and creates one PDF. Both include Titulna strana, then accounts in order;
Parametre and unrelated sheets are excluded. Signatures appear once at each
account's end, followed by location/date. Edits on Parametre refresh signatures while the add-in is loaded.
Ordinary Excel printing does not apply this worksheet selection.
These output commands require a workbook created with version 0.1.3 or later.
Create a new workbook with 0.1.5 for the new unmerged, monochrome portrait A4 layout.
Parametre is first; it remains excluded from ribbon Print/PDF.

FIRST CHECK
Import a known journal/general ledger. Select its correct fiscal year and
account names. Check account balances, filtered table totals and comments.
Workbooks are editable and are not protected. Save the resulting workbook.

UNINSTALL
Close Excel and run Uninstall.cmd. Workbooks and connection settings are kept.
This installer does not change the separate ExcelApiPoc.AddIn registration.

ADMINISTRATOR
The package installs only the Excel add-in, not the API or SQL database.
Deploy compatible API endpoints/database wrappers separately before MkSoft use.
Local files: %LOCALAPPDATA%\AccountBalanceVerification\AddIn
User settings: %APPDATA%\AccountBalanceVerification\settings.json

VERSION 0.1.5 CHECKS IN EXCEL
- Create a workbook: Parametre first, then Titulna strana and account tabs.
- Check black headers, white body/totals, no striping, and blank first total cell.
- Edit a name, role, print placement, location and dates on Parametre.
- Add/delete a people-table row; confirm signatures resize on title/accounts.
- An incomplete person shows a pending-refresh message in A9. Complete the row
  and confirm the message changes to successful refresh.
- Check portrait A4 Print Preview/PDF with long account names and large amounts.
  Width fits one page; long accounts may continue onto further pages.
- Names overflow into blank adjacent cells; very long names may still need
  manual column/font adjustment. There are no merged cells.
