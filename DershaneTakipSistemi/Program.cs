using DershaneTakipSistemi.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace DershaneTakipSistemi
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(connectionString));
            builder.Services.AddDatabaseDeveloperPageExceptionFilter();

            builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount =Sfalse) // RequireConfirmedAccount'ý false yapabiliriz, þimdilik e-posta doðrulamasýyla uðraþmayalým.
                .AddRoles<IdentityRole>() // <-- ROL YÖNETÝMÝNÝ EKLE
                .AddEntityFrameworkStores<ApplicationDbContext>();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseMigrationsEndPoint();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");
            app.MapRazorPages();

            // ===== Seed Data: Admin Rolü ve Kullanýcýsý Oluþturma =====
            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                try
                {
                    var userManager = services.GetRequiredService<UserManager<IdentityUser>>();
                    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

                    // Admin rolü yoksa oluþtur
                    string adminRoleName = "Admin";
                    if (!await roleManager.RoleExistsAsync(adminRoleName))
                    {
                        await roleManager.CreateAsync(new IdentityRole(adminRoleName));
                        Console.WriteLine($"'{adminRoleName}' rolü oluþturuldu."); // Konsola bilgi yazdýrabiliriz
                    }

                    // Admin kullanýcýsý yoksa oluþtur ve role ata
                    string adminEmail = "admin@dershane.com"; // Ýstediðin bir e-posta
                    string adminPassword = "Password123!";     // Güçlü bir þifre seç!

                    var adminUser = await userManager.FindByEmailAsync(adminEmail);
                    if (adminUser == null)
                    {
                        adminUser = new IdentityUser
                        {
                            UserName = adminEmail,
                            Email = adminEmail,
                            EmailConfirmed = true // E-posta doðrulamasý gerektirmediðimiz için true yapalým
                        };
                        var createUserResult = await userManager.CreateAsync(adminUser, adminPassword);

                        if (createUserResult.Succeeded)
                        {
                            Console.WriteLine($"'{adminEmail}' kullanýcýsý oluþturuldu.");
                            // Kullanýcýyý Admin rolüne ata
                            await userManager.AddToRoleAsync(adminUser, adminRoleName);
                            Console.WriteLine($"'{adminEmail}' kullanýcýsý '{adminRoleName}' rolüne atandý.");
                        }
                        else
                        {
                            // Hata durumunda loglama yapabilirsin
                            Console.WriteLine($"'{adminEmail}' kullanýcýsý oluþturulamadý: {string.Join(", ", createUserResult.Errors.Select(e => e.Description))}");
                        }
                    }
                    else
                    {
                        // Kullanýcý varsa ama rolde deðilse role ata (isteðe baðlý kontrol)
                        if (!await userManager.IsInRoleAsync(adminUser, adminRoleName))
                        {
                            await userManager.AddToRoleAsync(adminUser, adminRoleName);
                            Console.WriteLine($"Mevcut '{adminEmail}' kullanýcýsý '{adminRoleName}' rolüne atandý.");
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Hata olursa loglama yap
                    var logger = services.GetRequiredService<ILogger<Program>>();
                    logger.LogError(ex, "Baþlangýç verisi (seed) oluþturulurken bir hata oluþtu.");
                }
            }
            // ============================================================

            app.Run(); // Bu satýr en sonda kalmalý

        }
    }
}
