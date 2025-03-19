-- Ürün verileri için INSERT sorguları
-- Elektronik kategorisi ürünleri (CategoryId: 1)
INSERT INTO Products (Name, Description, Image, Stock, Price, IsHome, IsApproved, CategoryId) VALUES 
('Samsung Galaxy S24', 'Samsung''ın en yeni akıllı telefon modeli 6.1 inç ekrana sahip', '/images/products/phone1.jpg', 25, 24999.99, 1, 1, 1),
('Apple MacBook Pro', '13 inç M2 çipli MacBook Pro, 8GB RAM, 256GB SSD', '/images/products/laptop1.jpg', 10, 42999.99, 1, 1, 1),
('JBL Bluetooth Kulaklık', 'Kablosuz kulaklık, gürültü önleme teknolojisi', '/images/products/headphone1.jpg', 50, 1299.99, 0, 1, 1),
('Sony 4K Smart TV', '55 inç Android TV, HDR desteği', '/images/products/tv1.jpg', 15, 17499.99, 1, 1, 1),
('Canon EOS M50', 'Aynasız fotoğraf makinesi, 24.1MP', '/images/products/camera1.jpg', 8, 13999.99, 0, 1, 1);

-- Giyim kategorisi ürünleri (CategoryId: 2)
INSERT INTO Products (Name, Description, Image, Stock, Price, IsHome, IsApproved, CategoryId) VALUES 
('Erkek Deri Ceket', 'Suni deri ceket, siyah renk, slim fit', '/images/products/jacket1.jpg', 20, 899.99, 0, 1, 2),
('Kadın Triko Kazak', 'Boğazlı triko kazak, krema rengi', '/images/products/sweater1.jpg', 30, 349.99, 1, 1, 2),
('Kot Pantolon', 'Yüksek bel, mavi, straight fit', '/images/products/jeans1.jpg', 40, 299.99, 0, 1, 2),
('Spor Ayakkabı', 'Günlük kullanım için rahat spor ayakkabı', '/images/products/shoes1.jpg', 25, 599.99, 1, 1, 2),
('Şal Desenli Elbise', 'Yazlık elbise, çiçek deseni, midi boy', '/images/products/dress1.jpg', 15, 449.99, 0, 1, 2);

-- Ev & Yaşam kategorisi ürünleri (CategoryId: 3)
INSERT INTO Products (Name, Description, Image, Stock, Price, IsHome, IsApproved, CategoryId) VALUES 
('Modern Koltuk Takımı', '3+2+1 oturma grubu, gri kumaş', '/images/products/sofa1.jpg', 5, 12999.99, 1, 1, 3),
('Yemek Masası Seti', '6 kişilik ahşap masa ve sandalye seti', '/images/products/table1.jpg', 8, 5499.99, 0, 1, 3),
('Akıllı Robot Süpürge', 'Akıllı navigasyon, uzaktan kontrol', '/images/products/vacuum1.jpg', 12, 3499.99, 1, 1, 3),
('Yatak Örtüsü Takımı', 'Çift kişilik, pamuklu kumaş, mavi desen', '/images/products/bedding1.jpg', 20, 699.99, 0, 1, 3),
('LED Tavan Aydınlatma', 'Uzaktan kumandalı, renk değiştiren', '/images/products/light1.jpg', 30, 399.99, 0, 1, 3);

-- Kitap & Kırtasiye kategorisi ürünleri (CategoryId: 4)
INSERT INTO Products (Name, Description, Image, Stock, Price, IsHome, IsApproved, CategoryId) VALUES 
('Suç ve Ceza', 'Fyodor Dostoyevski, ciltli baskı', '/images/products/book1.jpg', 50, 89.99, 0, 1, 4),
('1984', 'George Orwell, karton kapak', '/images/products/book2.jpg', 45, 69.99, 1, 1, 4),
('Defter Seti', '3''lü çizgili defter, A5 boyut', '/images/products/notebook1.jpg', 100, 49.99, 0, 1, 4),
('Kalem Seti', '12 renkli kalem seti, su bazlı', '/images/products/pens1.jpg', 80, 34.99, 0, 1, 4),
('Akıllı Tablet Kalemi', 'Dijital çizim ve not alma kalemi', '/images/products/stylus1.jpg', 25, 399.99, 1, 1, 4);

-- Spor & Outdoor kategorisi ürünleri (CategoryId: 5)
INSERT INTO Products (Name, Description, Image, Stock, Price, IsHome, IsApproved, CategoryId) VALUES 
('Koşu Bandı', 'Katlanabilir, 12 program', '/images/products/treadmill1.jpg', 7, 7999.99, 1, 1, 5),
('Yoga Matı', 'Kaymaz, 5mm kalınlık, mor renk', '/images/products/yoga1.jpg', 40, 149.99, 0, 1, 5),
('Dumbbell Set', '2x5kg + 2x10kg ağırlık seti', '/images/products/weights1.jpg', 15, 899.99, 0, 1, 5),
('Kamp Çadırı', '4 kişilik, su geçirmez', '/images/products/tent1.jpg', 10, 1499.99, 1, 1, 5),
('Trekking Ayakkabısı', 'Su geçirmez, erkek, kahverengi', '/images/products/boots1.jpg', 20, 799.99, 0, 1, 5);

-- Kozmetik & Kişisel Bakım kategorisi ürünleri (CategoryId: 6)
INSERT INTO Products (Name, Description, Image, Stock, Price, IsHome, IsApproved, CategoryId) VALUES 
('Parfüm', 'Unisex, odunsu ve baharatlı notalar', '/images/products/perfume1.jpg', 30, 699.99, 1, 1, 6),
('Cilt Bakım Seti', 'Temizleyici, tonik ve nemlendirici içerir', '/images/products/skincare1.jpg', 25, 349.99, 0, 1, 6),
('Saç Düzleştirici', 'Seramik kaplama, ısı ayarlı', '/images/products/hair1.jpg', 18, 499.99, 0, 1, 6),
('Makyaj Paleti', '18 renk göz farı', '/images/products/makeup1.jpg', 35, 279.99, 1, 1, 6),
('Elektrikli Diş Fırçası', 'Şarj edilebilir, 3 fırça başlıklı', '/images/products/toothbrush1.jpg', 22, 449.99, 0, 1, 6); 