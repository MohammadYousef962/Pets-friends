using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Pets_friends.Data;
using Pets_friends.Models;

var builder = WebApplication.CreateBuilder(args);

// 1. Services
builder.Services.AddControllersWithViews();

// 2. Database
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// 3. Identity (Configured for easy testing during development)
builder.Services.AddIdentity<UserAccount, IdentityRole>(options =>
{
    options.SignIn.RequireConfirmedAccount = false; // No email verification required yet
    options.Password.RequireDigit = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequiredLength = 6;
})
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

var app = builder.Build();

// 4. Initialize Database (Our new custom seeder!)
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        // This triggers the class we made to create roles and the master admin
        await DbInitializer.SeedRolesAndAdminAsync(services);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while seeding the database.");
    }
}

// 5. Middleware Pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication(); // Must come BEFORE Authorization
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}"); // Set to start at Login

app.Run();