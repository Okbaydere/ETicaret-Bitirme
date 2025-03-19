# PowerShell script to download product images from Unsplash
Write-Host "ETicaret ürün resimlerini indirme işlemi başlatılıyor..." -ForegroundColor Cyan

# Create the target directory if it doesn't exist
$targetDir = "ETicaretUI\wwwroot\images\products"
if (-not (Test-Path $targetDir)) {
    New-Item -Path $targetDir -ItemType Directory -Force | Out-Null
    Write-Host "Klasör oluşturuldu: $targetDir" -ForegroundColor Green
} else {
    Write-Host "Klasör zaten mevcut: $targetDir" -ForegroundColor Yellow
}

# Define image mappings: filename -> URL
$imageMap = @{
    # Elektronik
    "phone1.jpg" = "https://images.unsplash.com/photo-1598327105666-5b89351aff97"
    "laptop1.jpg" = "https://images.unsplash.com/photo-1517336714731-489689fd1ca8"
    "headphone1.jpg" = "https://images.unsplash.com/photo-1505740420928-5e560c06d30e"
    "tv1.jpg" = "https://images.unsplash.com/photo-1593359677879-a4bb92f829d1"
    "camera1.jpg" = "https://images.unsplash.com/photo-1516035069371-29a1b244cc32"
    
    # Giyim
    "jacket1.jpg" = "https://images.unsplash.com/photo-1551028719-00167b16eac5"
    "sweater1.jpg" = "https://images.unsplash.com/photo-1576871337622-98d48d1cf531"
    "jeans1.jpg" = "https://images.unsplash.com/photo-1541099649105-f69ad21f3246"
    "shoes1.jpg" = "https://images.unsplash.com/photo-1542291026-7eec264c27ff"
    "dress1.jpg" = "https://images.unsplash.com/photo-1496747611176-843222e1e57c"
    
    # Ev & Yaşam
    "sofa1.jpg" = "https://images.unsplash.com/photo-1555041469-a586c61ea9bc"
    "table1.jpg" = "https://images.unsplash.com/photo-1615066390971-03e2e3931235"
    "vacuum1.jpg" = "https://images.unsplash.com/photo-1567690187548-f07b1d7bf5a9"
    "bedding1.jpg" = "https://images.unsplash.com/photo-1522771739844-6a9f6d5f14af"
    "light1.jpg" = "https://images.unsplash.com/photo-1603075600614-2f34e6d50500"
    
    # Kitap & Kırtasiye
    "book1.jpg" = "https://images.unsplash.com/photo-1544947950-fa07a98d237f"
    "book2.jpg" = "https://images.unsplash.com/photo-1532012197267-da84d127e765"
    "notebook1.jpg" = "https://images.unsplash.com/photo-1531346680769-a1d79b57de5c"
    "pens1.jpg" = "https://images.unsplash.com/photo-1586363104862-3a5e2ab60d99"
    "stylus1.jpg" = "https://images.unsplash.com/photo-1584812350699-3a753ea90c0b"
    
    # Spor & Outdoor
    "treadmill1.jpg" = "https://images.unsplash.com/photo-1570829460005-c840387bb1ca"
    "yoga1.jpg" = "https://images.unsplash.com/photo-1588286840104-8957b019727f"
    "weights1.jpg" = "https://images.unsplash.com/photo-1526506118085-60ce8714f8c5"
    "tent1.jpg" = "https://images.unsplash.com/photo-1504280390367-361c6d9f38f4"
    "boots1.jpg" = "https://images.unsplash.com/photo-1520219306100-ec69c7a73522"
    
    # Kozmetik & Kişisel Bakım
    "perfume1.jpg" = "https://images.unsplash.com/photo-1594035910387-fcd4c7f20181"
    "skincare1.jpg" = "https://images.unsplash.com/photo-1556760544-74068565f05c"
    "hair1.jpg" = "https://images.unsplash.com/photo-1522337360788-8b13dee7a37e"
    "makeup1.jpg" = "https://images.unsplash.com/photo-1596704017234-0ea97c9f8c8b"
    "toothbrush1.jpg" = "https://images.unsplash.com/photo-1559591937-abc5351051c5"
}

# Download each image
$totalImages = $imageMap.Count
$currentImage = 0
$successCount = 0

foreach ($image in $imageMap.GetEnumerator()) {
    $currentImage++
    $fileName = $image.Key
    $url = $image.Value
    $outputPath = Join-Path -Path $targetDir -ChildPath $fileName
    
    Write-Host "[$currentImage/$totalImages] İndiriliyor: $fileName" -NoNewline

    try {
        # Add parameters to get a reasonable sized image ~800px
        $downloadUrl = "$url`?auto=format&fit=crop&w=800&q=80"
        
        # Download the image
        Invoke-WebRequest -Uri $downloadUrl -OutFile $outputPath -UseBasicParsing
        Write-Host " ✓" -ForegroundColor Green
        $successCount++
    } catch {
        Write-Host " ✗" -ForegroundColor Red
        Write-Host "   Hata: $_" -ForegroundColor Red
    }
}

Write-Host ""
Write-Host "İndirme işlemi tamamlandı: $successCount başarılı, $($totalImages - $successCount) başarısız" -ForegroundColor Cyan
Write-Host "Resimler şu klasöre kaydedildi: $targetDir" -ForegroundColor Cyan
Write-Host ""
pause