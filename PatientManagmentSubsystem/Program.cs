using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PatientManagmentSubsystem.Data;
using PatientManagmentSubsystem.Models;

namespace PatientManagmentSubsystem
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(connectionString));

            // Use ApplicationUser (your custom user) instead of IdentityUser
            builder.Services.AddDefaultIdentity<Data.ApplicationUser>(options =>
            {
                options.SignIn.RequireConfirmedAccount = false; 
            })
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<ApplicationDbContext>();

            builder.Services.AddControllersWithViews();

            var app = builder.Build();

            // ----- Apply migrations & seed -----
            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                var db = services.GetRequiredService<ApplicationDbContext>();
                await db.Database.MigrateAsync();

                // Seed Roles & first WardAdmin
                var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
                var userManager = services.GetRequiredService<UserManager<Data.ApplicationUser>>();

                string[] roles = { "WardAdmin", "Doctor" };
                foreach (var r in roles)
                {
                    if (!await roleManager.RoleExistsAsync(r))
                        await roleManager.CreateAsync(new IdentityRole(r));
                }

                // First admin
                string adminEmail = "admin@hospital.com";
                var admin = await userManager.FindByEmailAsync(adminEmail);
                if (admin == null)
                {
                    admin = new Data.ApplicationUser
                    {
                        UserName = adminEmail,
                        Email = adminEmail,
                        FullName = "Main Ward Admin",
                        EmailConfirmed = true
                    };
                    await userManager.CreateAsync(admin, "Admin@123"); // strong password
                    await userManager.AddToRoleAsync(admin, "WardAdmin");
                }

                // Optional: initial ward/bed seeding (only if none exist)
                if (!db.Wards.Any())
                {
                    db.Wards.AddRange(
                        new Ward { Name = "General Ward", TotalBeds = 10, AvailableBeds = 10 },
                        new Ward { Name = "Emergency", TotalBeds = 5, AvailableBeds = 5 },
                        new Ward { Name = "ICU", TotalBeds = 4, AvailableBeds = 4 },
                        new Ward { Name = "Pediatric", TotalBeds = 6, AvailableBeds = 6 }
                    );
                    await db.SaveChangesAsync();
                }

                if (!db.Beds.Any())
                {
                    var wardIds = db.Wards.ToDictionary(w => w.Name, w => w.WardId);

                    db.Beds.AddRange(
                        new Bed { Name = "A-101", WardId = wardIds["General Ward"] },
                        new Bed { Name = "A-102", WardId = wardIds["General Ward"] },
                        new Bed { Name = "E-001", WardId = wardIds["Emergency"] },
                        new Bed { Name = "ICU-1", WardId = wardIds["ICU"] },
                        new Bed { Name = "P-201", WardId = wardIds["Pediatric"] }
                    );
                    await db.SaveChangesAsync();
                }
            }

            // --- Middleware ---
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();
            app.UseAuthentication();   
            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Dashboard}/{id?}");

            // Identity UI (Login/Register/Logout)
            app.MapRazorPages();

            await app.RunAsync();
        }
    }
}
