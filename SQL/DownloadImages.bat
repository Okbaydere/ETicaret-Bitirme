@echo off
echo ETicaret ürün resimlerini indirme işlemi başlatılıyor...
echo Bu işlem PowerShell kullanarak resimleri indirecektir.
echo.

REM Check if PowerShell is available
where powershell >nul 2>&1
if %ERRORLEVEL% NEQ 0 (
    echo HATA: PowerShell bulunamadı.
    echo Lütfen PowerShell'in yüklü olduğundan emin olun.
    goto :end
)

REM Run the PowerShell script
powershell -ExecutionPolicy Bypass -File "%~dp0DownloadImages.ps1"

:end
echo.
pause 