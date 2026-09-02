ExcelApiPoc.AddIn deployment
============================

Requirements
------------
- Windows desktop with 64-bit Microsoft 365 Excel.
- .NET Framework 4.8.
- Access to http://10.0.0.249:5080, locally or through WireGuard VPN.
- Microsoft Excel must be closed during installation, rollback, or uninstall.
- Administrator rights are not required.

Install or update
-----------------
1. Extract the complete ZIP file to a temporary local folder.
2. Close every Microsoft Excel window.
3. Double-click Install.cmd.
4. Start Excel.
5. Open ExcelApiPoc Settings and select Test connection.

The add-in is installed for the current Windows user under:
%LOCALAPPDATA%\ExcelApiPoc\AddIn\<version>

Existing settings and cached API data are preserved during an update. The API
URL is created as http://10.0.0.249:5080 only when settings.json does not yet
exist.

Rollback
--------
Close Excel and double-click Rollback.cmd. Rollback is available after an update
when the immediately preceding version still exists on the computer.

Uninstall
---------
Close Excel and double-click Uninstall.cmd. This removes the Excel registration
and installed add-in versions. Settings and cached data are preserved.

To remove settings and cached data as well, run PowerShell from this directory:

PowerShell.exe -NoProfile -ExecutionPolicy Bypass -File .\Uninstall.ps1 -RemoveUserData

Pilot verification
------------------
1. Confirm Install.cmd completes without an error.
2. Confirm the ExcelApiPoc ribbon appears after Excel starts.
3. Confirm Settings > Test connection succeeds.
4. Create and recalculate a small audit workbook.
5. Close and reopen Excel and confirm the ribbon loads again.
6. Verify the same API URL while connected through WireGuard outside the office.

