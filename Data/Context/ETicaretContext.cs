using Data.Entities;
using Data.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Data.Context;

public class ETicaretContext : IdentityDbContext<AppUser, AppRole, int>
{
    // DbContextOptions parametresi alan constructor
    public ETicaretContext(DbContextOptions<ETicaretContext> options) : base(options)
    {
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        // Eğer DI (Dependency Injection) ile options gelmediyse, varsayılan bağlantıyı kullan
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseSqlServer(
                   //"Server=tcp:10.20.103.33,1433;Database=ETicaret;User ID=SA;Password=Softito1882;Trusted_Connection=False;Encrypt=False;");
                   "Server=DESKTOP-TUBBJ3B;Database=ETicaret;Trusted_Connection=True;");
        }
        base.OnConfiguring(optionsBuilder);
    }

    public DbSet<Category> Categories { get; set; } = null!;
    public DbSet<Product> Products { get; set; } = null!;
    public DbSet<Order> Orders { get; set; } = null!;
    public DbSet<OrderLine> OrderLines { get; set; } = null!;
    public DbSet<Cart> Carts { get; set; } = null!;
    public DbSet<CartItem> CartItems { get; set; } = null!;
    public DbSet<Address> Addresses { get; set; } = null!;
}