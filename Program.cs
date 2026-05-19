using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Pets_friends.Data;
using Pets_friends.Models;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

// ====================================================
// PART 1: CONFIGURE SERVICES (The App's Toolbox)
// ====================================================

builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// 1. Identity & Password Rules
builder.Services.AddIdentity<UserAccount, IdentityRole>(options =>
{
    options.User.RequireUniqueEmail = true;
    options.SignIn.RequireConfirmedAccount = false;

    options.Password.RequireDigit = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireLowercase = true;
    options.Password.RequiredLength = 6;

    // --- FORCE IDENTITY TO USE OUR NEW CUSTOM PROVIDER ---
    options.Tokens.PasswordResetTokenProvider = "PetFriendsResetProvider";
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders()
// --- REGISTER OUR CUSTOM PROVIDER ---
.AddTokenProvider<PetFriendsTokenProvider<UserAccount>>("PetFriendsResetProvider");

// 2. Security Tokens & Cookies
// --- SET THE STRICT 1-MINUTE RULE FOR OUR CUSTOM PROVIDER ---
builder.Services.Configure<PetFriendsTokenProviderOptions>(options =>
{
    options.TokenLifespan = TimeSpan.FromMinutes(1); // Set to 1 minute for testing!
});

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.ExpireTimeSpan = TimeSpan.FromDays(30);
    options.SlidingExpiration = true;
});

// ====================================================
// PART 2: BUILD APP & CONFIGURE PIPELINE
// ====================================================

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        await DbInitializer.SeedRolesAndAdminAsync(services);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while seeding the database.");
    }
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseDeveloperExceptionPage();
app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Main}/{id?}");

app.Run();

// ====================================================
// PART 3: OUR CUSTOM BULLETPROOF TOKEN GENERATOR
// ====================================================
public class PetFriendsTokenProvider<TUser> : DataProtectorTokenProvider<TUser> where TUser : class
{
    public PetFriendsTokenProvider(
        IDataProtectionProvider dataProtectionProvider,
        IOptions<PetFriendsTokenProviderOptions> options,
        ILogger<DataProtectorTokenProvider<TUser>> logger)
        : base(dataProtectionProvider, options, logger)
    { }
}

public class PetFriendsTokenProviderOptions : DataProtectionTokenProviderOptions { }