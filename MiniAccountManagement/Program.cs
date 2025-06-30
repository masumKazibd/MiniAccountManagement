using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MiniAccountManagement.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

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

// Seed roles
string[] roleNames = { "Admin", "Accountant", "Viewer" };
foreach (var roleName in roleNames)
{
    if (!await roleManager.RoleExistsAsync(roleName))
    {
        await roleManager.CreateAsync(new IdentityRole(roleName));
    }
}

// Seed an admin user
var adminUserEmail = "admin@example.com";
if (await userManager.FindByEmailAsync(adminUserEmail) == null)
{
    var adminUser = new IdentityUser
    {
        UserName = adminUserEmail,
        Email = adminUserEmail
    };
    await userManager.CreateAsync(adminUser, "admin");
    await userManager.AddToRoleAsync(adminUser, "Admin");
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
