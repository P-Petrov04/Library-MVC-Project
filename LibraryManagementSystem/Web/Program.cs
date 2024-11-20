using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Web.Data;

namespace Web
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Configure Database and Identity
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(connectionString));

            builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
            {
                options.SignIn.RequireConfirmedAccount = false; // Disable email confirmation
            })
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

            builder.Services.AddControllersWithViews();

            // Configure Session
            builder.Services.AddDistributedMemoryCache();
            builder.Services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(30);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });

            var app = builder.Build();

            // Seed Roles and Admin Users
            using (var scope = app.Services.CreateScope())
            {
                var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
                var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                SeedRolesAndAdmin(roleManager, userManager, dbContext).Wait();
            }

            // Configure Middleware
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();
            app.UseSession(); // Enable Session Middleware
            app.UseAuthentication(); // Enable Authentication
            app.UseAuthorization(); // Enable Authorization

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();
        }

        private static async Task SeedRolesAndAdmin(RoleManager<IdentityRole> roleManager, UserManager<IdentityUser> userManager, ApplicationDbContext context)
        {
            // Seed Roles
            string[] roles = { "Admin", "User", "Moderator" };

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    Console.WriteLine($"Creating role: {role}");
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            // Seed Admin Users from ApplicationDbContext
            var adminUsers = context.User.Where(u => u.RoleId == 1).ToList();

            if (!adminUsers.Any())
            {
                Console.WriteLine("No Admin users found in the User table.");
                return;
            }

            foreach (var admin in adminUsers)
            {
                Console.WriteLine($"Processing admin: {admin.Email}");

                // Check if the user exists in Identity
                var identityUser = await userManager.FindByEmailAsync(admin.Email);
                if (identityUser == null)
                {
                    // Create IdentityUser for this admin
                    identityUser = new IdentityUser
                    {
                        UserName = admin.Email,
                        Email = admin.Email,
                        EmailConfirmed = true
                    };

                    // Hash the password from the `User` table
                    string hashedPassword = userManager.PasswordHasher.HashPassword(identityUser, admin.Password);

                    // Set the hashed password
                    identityUser.PasswordHash = hashedPassword;

                    // Add the user to Identity
                    var result = await userManager.CreateAsync(identityUser);

                    if (result.Succeeded)
                    {
                        Console.WriteLine($"Assigning Admin role to: {admin.Email}");
                        await userManager.AddToRoleAsync(identityUser, "Admin");
                    }
                    else
                    {
                        Console.WriteLine($"Error creating admin {admin.Email}: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                    }
                }
                else
                {
                    Console.WriteLine($"Admin already exists: {admin.Email}");
                }
            }
        }
    }
}
