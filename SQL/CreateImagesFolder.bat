@echo off
echo Ürün resimleri için klasör oluşturuluyor...

REM Proje klasörünü kontrol et
IF NOT EXIST "ETicaretUI" (
    echo ETicaretUI klasörü bulunamadı.
    echo Bu dosyayı projenin ana dizininde çalıştırdığınızdan emin olun.
    pause
    exit /b
)

REM Klasör yolunu oluştur
set "FOLDER=ETicaretUI\wwwroot\images\products"

REM Klasörü oluştur
mkdir "%FOLDER%" 2>nul

IF EXIST "%FOLDER%" (
    echo.
    echo Klasör başarıyla oluşturuldu: %FOLDER%
    echo.
    echo Lütfen SQL\ImageResources.md dosyasındaki linkleri kullanarak
    echo ürün resimlerini bu klasöre indirin.
) ELSE (
    echo.
    echo Klasör oluşturulamadı: %FOLDER%
    echo Lütfen manuel olarak oluşturmayı deneyin.
)

echo.
pause 