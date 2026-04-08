using Microsoft.EntityFrameworkCore;
using EMS.DAL.Data;
using EMS.DAL.Repository;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

// Get connection string from environment variable
var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection");

if (string.IsNullOrEmpty(connectionString))
{
    connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
}

// Convert URL format to key-value format for PostgreSQL
if (connectionString != null && connectionString.StartsWith("postgresql://"))
{
    try
    {
        var uri = new Uri(connectionString);
        var userInfo = uri.UserInfo.Split(':');
        var username = userInfo[0];
        var password = userInfo[1];
        var database = uri.AbsolutePath.TrimStart('/');
        connectionString = $"Host={uri.Host};Database={database};Username={username};Password={password};Port={uri.Port};SSL Mode=Require;Trust Server Certificate=true;";
        Console.WriteLine("Converted PostgreSQL connection string");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error parsing connection string: {ex.Message}");
    }
}

Console.WriteLine($"Connection string (first 50 chars): {(connectionString?.Length > 50 ? connectionString.Substring(0, 50) : connectionString)}");

// Add DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseNpgsql(connectionString);
    options.EnableSensitiveDataLogging(true);
});

// Register repositories
builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
builder.Services.AddScoped<IEventRepository, EventRepository>();
builder.Services.AddScoped<ISessionRepository, SessionRepository>();
builder.Services.AddScoped<IParticipantRepository, ParticipantRepository>();

// Authentication
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

// Create database and tables
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    try
    {
        dbContext.Database.EnsureCreated();
        Console.WriteLine("Database and tables created/verified successfully!");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Database error details: {ex.Message}");
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