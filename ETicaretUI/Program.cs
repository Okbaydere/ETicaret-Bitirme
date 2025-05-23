using Dal.Abstract;
using Dal.Concrete;
using Data.Context;
using Data.Identity;
using ETicaretUI.SeedData;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

//Dependency Injection
builder.Services.AddDbContext<ETicaretContext>();
builder.Services.AddScoped<ICategoryDal, CategoryDal>();
builder.Services.AddScoped<IProductDal, ProductDal>();
builder.Services.AddScoped<IOrderDal, OrderDal>();
builder.Services.AddScoped<IOrderLineDal, OrderLineDal>();
builder.Services.AddScoped<ICartDal, CartDal>();
builder.Services.AddScoped<ICartItemDal, CartItemDal>();
builder.Services.AddScoped<IAddressDal, AddressDal>();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.AccessDeniedPath = new PathString("/Account/AccessDenied");
        options.LoginPath = new PathString("/Account/Login");
    });

//Identity Kimlik Doğrulama
builder.Services.AddIdentity<AppUser, AppRole>(option =>
    {
        option.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15); //Kilitleme süresi
        option.Lockout.MaxFailedAccessAttempts = 5; //Max yanlış giriş
        option.Password.RequireDigit = false; // Rakam gerekliliği kaldırıldı
        option.Password.RequireNonAlphanumeric = false; // Özel karakter gereksinimi kaldırıldı
        option.Password.RequireLowercase = false; // Küçük harf gereksinimi kaldırıldı
        option.Password.RequireUppercase = false; // Büyük harf gereksinimi kaldırıldı
    }).AddEntityFrameworkStores<ETicaretContext>() // Ef ile veri tabanı bağlantısını sağlar
    .AddDefaultTokenProviders(); //Parolaları sıfırlama, e-posta değiştirme
//ve telefon numarası değiştirme işlemleri
//ve iki faktörlü kimlik doğrulama belirteci
//oluşturma için belirteç oluşturmak üzere
//kullanılan varsayılan belirteç sağlayıcılarını ekler
builder.Services.ConfigureApplicationCookie(option =>
{
    option.LoginPath = "/Account/Login"; // giriş yapılmadığında 
    option.AccessDeniedPath = "/Account/AccessDenied"; //yetkisiz erişim
    option.LogoutPath = "/Account/Logout";
    option.Cookie = new CookieBuilder
    {
        Name = "AspNetCoreIdentityExampleCookie", //çerez ismi, sistemden gelen isimmiş biz vermedik
        HttpOnly = false, //çerez http
        SameSite = SameSiteMode.Lax, // Aynı sitede yapılan isteklerde geçerli çerez
        SecurePolicy = CookieSecurePolicy.Always, //SSL sertifikası olmayan sitelere izin yok
    };
    option.SlidingExpiration = true; //Çerez geçerlilik süresi doldukça yenilenir
    option.ExpireTimeSpan = TimeSpan.FromMinutes(15); //çerez geçerlilik süresi
});


//Oturum yönetimi
builder.Services.AddSession(); //oturum yönetim servisi
var app = builder.Build(); //Hata verirse yerini değiştir ?


// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection(); //http'den https'e yönlendirir
app.UseStaticFiles();

app.UseRouting();
app.UseAuthentication(); //kimlik doğrulama
app.UseAuthorization(); //Yetkilendirme işlemi
app.UseSession(); //oturum yönetimini aktifleştir

// Seed verileri oluştur
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        // Kullanıcı ve rolleri oluştur
        await SeedData.InitializeAsync(services);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Seed verileri oluşturulurken bir hata oluştu.");
    }
}

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=List}/{id?}");

app.Run();