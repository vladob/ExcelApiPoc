INVENTARIZACIA - Account Balance Verification 0.1.2 (pilot)

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
detected identity/year and select the official account framework.
Both MkSoft and file imports need API access. File imports retrieve the official
account framework from the API. localhost refers to your own computer and is
not the office server address. Connect to the office network/VPN as required.
No API keys or personal settings are included in this package.

FIRST CHECK
Import a known journal/general ledger. Select its correct fiscal year and
account framework. Check account balances, filtered table totals and comments.
Workbooks are editable and are not protected. Save the resulting workbook.

UNINSTALL
Close Excel and run Uninstall.cmd. Workbooks and connection settings are kept.
This installer does not change the separate ExcelApiPoc.AddIn registration.

ADMINISTRATOR
The package installs only the Excel add-in, not the API or SQL database.
Deploy compatible API endpoints/database wrappers separately before MkSoft use.
Local files: %LOCALAPPDATA%\AccountBalanceVerification\AddIn
User settings: %APPDATA%\AccountBalanceVerification\settings.json
