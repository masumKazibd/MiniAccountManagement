using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using MiniAccountManagement.Data;
using System.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddTransient<IDbConnection>(sp =>
{
    var configuration = sp.GetRequiredService<IConfiguration>();
    var connectionString = configuration.GetConnectionString("DefaultConnection");
    return new SqlConnection(connectionString);
}); 

builder.Services.AddDefaultIdentity<IdentityUser>(options =>
{
    options.Password.RequireDigit = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 6;
}).AddRoles<IdentityRole>()
.AddEntityFrameworkStores<ApplicationDbContext>();
builder.Services.AddRazorPages();

var app = builder.Build();

var scope = app.Services.CreateScope();
var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();

string[] roleNames = { "Admin", "Accountant", "Viewer" };
foreach (var roleName in roleNames)
{
    if (!await roleManager.RoleExistsAsync(roleName))
    {
        await roleManager.CreateAsync(new IdentityRole(roleName));
    }
}

var adminUserEmail = "admin@example.com";
var adminUser = await userManager.FindByEmailAsync(adminUserEmail);
if (adminUser == null)
{
    adminUser = new IdentityUser
    {
        UserName = adminUserEmail,
        Email = adminUserEmail
    };

    var createUserResult = await userManager.CreateAsync(adminUser, "admin123");
    if (!createUserResult.Succeeded)
    {
        var errors = string.Join("; ", createUserResult.Errors.Select(e => e.Description));
        throw new Exception($"Failed to create admin user: {errors}");
    }
}

if (!await userManager.IsInRoleAsync(adminUser, "Admin"))
{
    var addToRoleResult = await userManager.AddToRoleAsync(adminUser, "Admin");
    if (!addToRoleResult.Succeeded)
    {
        var errors = string.Join("; ", addToRoleResult.Errors.Select(e => e.Description));
        throw new Exception($"Failed to assign role: {errors}");
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();
app.Run();
