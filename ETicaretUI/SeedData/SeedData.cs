using Data.Identity;
using Microsoft.AspNetCore.Identity;

namespace ETicaretUI.SeedData
{
    public static class SeedData
    {
        public static async Task InitializeAsync(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<AppRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<AppUser>>();

            // Rolleri oluştur
            await CreateRolesAsync(roleManager);

            // Kullanıcıları oluştur
            await CreateUsersAsync(userManager);
        }

        private static async Task CreateRolesAsync(RoleManager<AppRole> roleManager)
        {
            // Admin rolü
            if (!await roleManager.RoleExistsAsync("Admin"))
            {
                var adminRole = new AppRole { Name = "Admin" };
                await roleManager.CreateAsync(adminRole);
            }

            // Normal kullanıcı rolü
            if (!await roleManager.RoleExistsAsync("User"))
            {
                var userRole = new AppRole { Name = "User" };
                await roleManager.CreateAsync(userRole);
            }
        }

        private static async Task CreateUsersAsync(UserManager<AppUser> userManager)
        {
            // Admin kullanıcısı
            if (await userManager.FindByNameAsync("admin") == null)
            {
                var adminUser = new AppUser
                {
                    UserName = "admin",
                    Email = "admin@example.com",
                    FirstName = "Admin",
                    LastName = "User"
                };

                var result = await userManager.CreateAsync(adminUser, "admin123");

                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                }
            }

            // Normal kullanıcı
            if (await userManager.FindByNameAsync("normal") == null)
            {
                var normalUser = new AppUser
                {
                    UserName = "normal",
                    Email = "normal@example.com",
                    FirstName = "Normal",
                    LastName = "User"
                };

                var result = await userManager.CreateAsync(normalUser, "normal123");

                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(normalUser, "User");
                }
            }
        }
    }
}