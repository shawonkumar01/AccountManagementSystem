using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using AccountManagementSystem.Data;

var builder = WebApplication.CreateBuilder(args);

// ✅ Set connection string (rename key if needed)
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
                      ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

// ✅ Register ApplicationDbContext
builder.Services.AddDbContext<AccountManagementSystemContext>(options =>
    options.UseSqlServer(connectionString));

// ✅ Register Identity with Roles
builder.Services.AddDefaultIdentity<IdentityUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
})
.AddRoles<IdentityRole>()
.AddEntityFrameworkStores<AccountManagementSystemContext>();

// ✅ Add Razor Pages
builder.Services.AddRazorPages();

var app = builder.Build();

// ✅ Seed Roles BEFORE app.Run
using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    string[] roles = { "Admin", "Accountant", "Viewer" };

    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
        {
            await roleManager.CreateAsync(new IdentityRole(role));
        }
    }
}

// ✅ Middleware pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthentication(); // 🧠 Don't forget this!
app.UseAuthorization();

app.MapRazorPages();

app.Run();
