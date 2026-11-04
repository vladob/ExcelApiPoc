@echo off
PowerShell.exe -NoProfile -ExecutionPolicy Bypass -File "%~dp0Uninstall.ps1"
set result=%errorlevel%
echo.
pause
exit /b %result%
