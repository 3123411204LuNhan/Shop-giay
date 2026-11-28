@echo off
cd /d "%~dp0"
dotnet run --project src\ShopGiay.Web -- --db-config
if errorlevel 1 goto end
dotnet run --project src\ShopGiay.Web -- --db-check
:end
pause
