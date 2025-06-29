using Efood_Menu.Models;
using Efood_Menu.Repositories;
using Microsoft.AspNetCore.DataProtection;
﻿using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Infrastructure;

var builder = WebApplication.CreateBuilder(args);
QuestPDF.Settings.License = LicenseType.Community;
// DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
	options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
	options.SignIn.RequireConfirmedAccount = false;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders()
.AddDefaultUI();
builder.Services.ConfigureApplicationCookie(options =>
{
    options.AccessDeniedPath = "/Account/AccessDenied";
});

// Razor Pages + MVC
builder.Services.AddControllers();
builder.Services.AddRazorPages();
builder.Services.AddControllersWithViews();

// Session
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Set session timeout as needed
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});
builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(@"C:\DataProtection-Keys"))
    .SetApplicationName("Efood_Menu");
// Repository
builder.Services.AddScoped<IFoodItemRepository, EFFoodItemRepository>();
builder.Services.AddScoped<ICategoryRepository, EFCategoryRepository>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
	app.UseExceptionHandler("/Home/Error");
	app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseSession();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();
app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Dashboard}/{action=Index}/{id?}");

app.MapControllerRoute(
	name: "default",
	pattern: "{controller=Home}/{action=Index}/{id?}");


app.MapRazorPages();

app.MapGet("/", context =>
{
    context.Response.Redirect("/Home/Index");
    return Task.CompletedTask;
});

// --- Seed Roles and Default Users ---
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

    await SeedRolesAndUsersAsync(userManager, roleManager);
}

static async Task SeedRolesAndUsersAsync(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
{
    // Roles
    var roles = new[] { SD.Role_Admin, SD.Role_Employee, SD.Role_Customer };
    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
            await roleManager.CreateAsync(new IdentityRole(role));
    }

    // Admin
    var adminEmail = "admin@efood.com";
    if (await userManager.FindByEmailAsync(adminEmail) == null)
    {
        var admin = new ApplicationUser
        {
            UserName = adminEmail,
            Email = adminEmail,
            FullName = "Admin User",
            EmailConfirmed = true
        };
        await userManager.CreateAsync(admin, "Admin@123");
        await userManager.AddToRoleAsync(admin, SD.Role_Admin);
    }

    // Employee
    var employeeEmail = "employee@efood.com";
    if (await userManager.FindByEmailAsync(employeeEmail) == null)
    {
        var employee = new ApplicationUser
        {
            UserName = employeeEmail,
            Email = employeeEmail,
            FullName = "Employee User",
            EmailConfirmed = true
        };
        await userManager.CreateAsync(employee, "Employee@123");
        await userManager.AddToRoleAsync(employee, SD.Role_Employee);
    }

    // Customer
    var userEmail = "user@efood.com";
    if (await userManager.FindByEmailAsync(userEmail) == null)
    {
        var user = new ApplicationUser
        {
            UserName = userEmail,
            Email = userEmail,
            FullName = "Regular User",
            EmailConfirmed = true
        };
        await userManager.CreateAsync(user, "User@123");
        await userManager.AddToRoleAsync(user, SD.Role_Customer);
    }
}

app.Run();
