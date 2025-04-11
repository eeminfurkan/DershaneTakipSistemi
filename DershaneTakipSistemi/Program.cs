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

            builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount =Sfalse) // RequireConfirmedAccount'� false yapabiliriz, �imdilik e-posta do�rulamas�yla u�ra�mayal�m.
                .AddRoles<IdentityRole>() // <-- ROL Y�NET�M�N� EKLE
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

            // ===== Seed Data: Admin Rol� ve Kullan�c�s� Olu�turma =====
            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                try
                {
                    var userManager = services.GetRequiredService<UserManager<IdentityUser>>();
                    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

                    // Admin rol� yoksa olu�tur
                    string adminRoleName = "Admin";
                    if (!await roleManager.RoleExistsAsync(adminRoleName))
                    {
                        await roleManager.CreateAsync(new IdentityRole(adminRoleName));
                        Console.WriteLine($"'{adminRoleName}' rol� olu�turuldu."); // Konsola bilgi yazd�rabiliriz
                    }

                    // Admin kullan�c�s� yoksa olu�tur ve role ata
                    string adminEmail = "admin@dershane.com"; // �stedi�in bir e-posta
                    string adminPassword = "Password123!";     // G��l� bir �ifre se�!

                    var adminUser = await userManager.FindByEmailAsync(adminEmail);
                    if (adminUser == null)
                    {
                        adminUser = new IdentityUser
                        {
                            UserName = adminEmail,
                            Email = adminEmail,
                            EmailConfirmed = true // E-posta do�rulamas� gerektirmedi�imiz i�in true yapal�m
                        };
                        var createUserResult = await userManager.CreateAsync(adminUser, adminPassword);

                        if (createUserResult.Succeeded)
                        {
                            Console.WriteLine($"'{adminEmail}' kullan�c�s� olu�turuldu.");
                            // Kullan�c�y� Admin rol�ne ata
                            await userManager.AddToRoleAsync(adminUser, adminRoleName);
                            Console.WriteLine($"'{adminEmail}' kullan�c�s� '{adminRoleName}' rol�ne atand�.");
                        }
                        else
                        {
                            // Hata durumunda loglama yapabilirsin
                            Console.WriteLine($"'{adminEmail}' kullan�c�s� olu�turulamad�: {string.Join(", ", createUserResult.Errors.Select(e => e.Description))}");
                        }
                    }
                    else
                    {
                        // Kullan�c� varsa ama rolde de�ilse role ata (iste�e ba�l� kontrol)
                        if (!await userManager.IsInRoleAsync(adminUser, adminRoleName))
                        {
                            await userManager.AddToRoleAsync(adminUser, adminRoleName);
                            Console.WriteLine($"Mevcut '{adminEmail}' kullan�c�s� '{adminRoleName}' rol�ne atand�.");
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Hata olursa loglama yap
                    var logger = services.GetRequiredService<ILogger<Program>>();
                    logger.LogError(ex, "Ba�lang�� verisi (seed) olu�turulurken bir hata olu�tu.");
                }
            }
            // ============================================================

            app.Run(); // Bu sat�r en sonda kalmal�

        }
    }
}
