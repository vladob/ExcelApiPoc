@echo off
PowerShell.exe -NoProfile -ExecutionPolicy Bypass -File "%~dp0Install.ps1"
set result=%errorlevel%
echo.
pause
exit /b %result%
