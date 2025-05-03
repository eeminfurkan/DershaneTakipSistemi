using DershaneTakipSistemi.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
// using Microsoft.AspNetCore.Mvc.Razor; // Bu genellikle gerekmez, kaldýrýlabilir.
using System.Threading.Tasks; // async Task Main için
using Microsoft.Extensions.Logging; // ILogger için
using System; // Exception için
using System.Linq; // LINQ metotlarý için (Select vb.)


namespace DershaneTakipSistemi // Kendi namespace'inizi kontrol edin
{
    public class Program
    {
        public static async Task Main(string[] args) // async Task olarak deðiþtirildi
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            // 1. Veritabaný Baðlantýsý ve DbContext
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(connectionString));

            // Geliþtirme ortamý için veritabaný hata sayfasý
            builder.Services.AddDatabaseDeveloperPageExceptionFilter();

            // 2. Identity Servisleri (Rollerle birlikte)
            builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = false)
                .AddRoles<IdentityRole>() // Rol yönetimi
                .AddEntityFrameworkStores<ApplicationDbContext>();

            // 3. Repository Servisleri
            //builder.Services.AddScoped<IOgrenciRepository, EfOgrenciRepository>(); // Öðrenci Repository Kaydý
            // Buraya ileride IOdemeRepository vb. eklenecek

            // 4. MVC Controller ve View Servisleri
            builder.Services.AddControllersWithViews();
            builder.Services.AddRazorPages(); // Identity UI için Razor Pages desteði

            // --- builder.Services ile ilgili eklemeler buraya ---


            var app = builder.Build(); // Uygulamayý oluþtur

            // Configure the HTTP request pipeline (Middleware Sýrasý Önemli!).
            if (app.Environment.IsDevelopment())
            {
                app.UseMigrationsEndPoint(); // Geliþtirme ortamýnda migration endpoint'i
            }
            else
            {
                app.UseExceptionHandler("/Home/Error"); // Hata yönetimi
                app.UseHsts(); // HTTPS Strict Transport Security
            }

            app.UseHttpsRedirection(); // HTTPS yönlendirmesi
            app.UseStaticFiles(); // CSS, JS, Resim gibi statik dosyalar için

            app.UseRouting(); // Yönlendirme middleware'i

            app.UseAuthentication(); // Kimlik Doðrulama middleware'i (Authorization'dan önce!)
            app.UseAuthorization(); // Yetkilendirme middleware'i

            // Endpoint Mapping
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");
            app.MapRazorPages(); // Identity UI sayfalarý için endpointler

            // ----- Seed Data Kodu -----
            // (app nesnesi oluþturulduktan ve pipeline yapýlandýrýldýktan sonra, app.Run() öncesi)
            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                try
                {
                    var userManager = services.GetRequiredService<UserManager<IdentityUser>>();
                    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
                    var logger = services.GetRequiredService<ILogger<Program>>(); // Logger'ý baþta alalým

                    // Admin rolü yoksa oluþtur
                    string adminRoleName = "Admin";
                    if (!await roleManager.RoleExistsAsync(adminRoleName))
                    {
                        await roleManager.CreateAsync(new IdentityRole(adminRoleName));
                        logger.LogInformation($"'{adminRoleName}' rolü oluþturuldu."); // Loglama kullanýmý
                    }

                    // Admin kullanýcýsý yoksa oluþtur ve role ata
                    string adminEmail = "admin@dershane.com";
                    string adminPassword = "Password123!"; // Daha güçlü bir þifre kullanýn!

                    var adminUser = await userManager.FindByEmailAsync(adminEmail);
                    if (adminUser == null)
                    {
                        adminUser = new IdentityUser
                        {
                            UserName = adminEmail,
                            Email = adminEmail,
                            EmailConfirmed = true
                        };
                        var createUserResult = await userManager.CreateAsync(adminUser, adminPassword);

                        if (createUserResult.Succeeded)
                        {
                            logger.LogInformation($"'{adminEmail}' kullanýcýsý oluþturuldu.");
                            await userManager.AddToRoleAsync(adminUser, adminRoleName);
                            logger.LogInformation($"'{adminEmail}' kullanýcýsý '{adminRoleName}' rolüne atandý.");
                        }
                        else
                        {
                            logger.LogError($"'{adminEmail}' kullanýcýsý oluþturulamadý: {string.Join(", ", createUserResult.Errors.Select(e => e.Description))}");
                        }
                    }
                    else
                    {
                        if (!await userManager.IsInRoleAsync(adminUser, adminRoleName))
                        {
                            await userManager.AddToRoleAsync(adminUser, adminRoleName);
                            logger.LogInformation($"Mevcut '{adminEmail}' kullanýcýsý '{adminRoleName}' rolüne atandý.");
                        }
                    }
                }
                catch (Exception ex)
                {
                    var logger = services.GetRequiredService<ILogger<Program>>();
                    logger.LogError(ex, "Baþlangýç verisi (seed) oluþturulurken bir hata oluþtu.");
                }
            }
            // --------------------------

            app.Run(); // Uygulamayý çalýþtýr
        }
    }
}