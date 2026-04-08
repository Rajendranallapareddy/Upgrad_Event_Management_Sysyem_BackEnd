using Microsoft.EntityFrameworkCore;
using EMS.DAL.Data;
using EMS.DAL.Repository;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

// Get connection string - with better error handling
var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection");

// Log for debugging
Console.WriteLine($"Environment variable exists: {!string.IsNullOrEmpty(connectionString)}");

if (string.IsNullOrEmpty(connectionString))
{
    connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    Console.WriteLine($"Using appsettings connection string: {!string.IsNullOrEmpty(connectionString)}");
}

// For Render PostgreSQL, ensure proper format
if (!string.IsNullOrEmpty(connectionString) && !connectionString.Contains("Host="))
{
    // Convert if needed - Render provides standard PostgreSQL URL
    // The URL format is already correct for Npgsql
}

Console.WriteLine($"Final connection string length: {connectionString?.Length ?? 0}");

// Use PostgreSQL
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    if (string.IsNullOrEmpty(connectionString))
    {
        throw new Exception("Database connection string is missing! Set ConnectionStrings__DefaultConnection environment variable.");
    }
    options.UseNpgsql(connectionString);
});

builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
builder.Services.AddScoped<IEventRepository, EventRepository>();
builder.Services.AddScoped<ISessionRepository, SessionRepository>();
builder.Services.AddScoped<IParticipantRepository, ParticipantRepository>();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
    });

builder.Services.AddHttpContextAccessor();
builder.Services.AddSession();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// Ensure database is created
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    try
    {
        dbContext.Database.EnsureCreated();
        Console.WriteLine("PostgreSQL database connected successfully!");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Database error: {ex.Message}");
        throw; // Re-throw to see the error in logs
    }
}

app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.UseSession();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();