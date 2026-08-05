using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SmartReportLog.Components;
using SmartReportLog.Endpoints;
using SmartReportLog.Entity.Identity;
using SmartReportLog.Persistance;
using SmartReportLog.Services.atm.Command.SaveData;
using SmartReportLog.Services.atm.Query;
using SmartReportLog.Services.Auth;
using SmartReportLog.Services.Ticket;
using SmartReportLog.Services.Users;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddDbContext<SmartLogContext>(x =>
{
    x.UseSqlServer(builder.Configuration.GetConnectionString("ApplicationDbContext")).EnableSensitiveDataLogging().LogTo(Console.WriteLine, LogLevel.Information); ;
});
builder.Services.AddDbContext<ErDbContext>(x =>
    x.UseSqlServer(builder.Configuration.GetConnectionString("ErDbConnection"),
        sql => sql.EnableRetryOnFailure(3, TimeSpan.FromSeconds(2), null)));

builder.Services.AddScoped<ITicketInfoProvider, SqlTicketInfoProvider>();
builder.Services.AddScoped<IAtmIngestionService,AtmIngestionService>();
builder.Services.AddScoped<IAtmQueryService, AtmQueryService>();
builder.Services.AddScoped<ITicketSyncService, TicketSyncService>();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IUserAdminService, UserAdminService>();

builder.Services.AddIdentity<AppUser, IdentityRole>(o =>
{
    o.Password.RequiredLength = 7;
    o.Password.RequireDigit = false;
    o.Password.RequireLowercase = false;
    o.Password.RequireUppercase = false;
    o.Password.RequireNonAlphanumeric = false;

    o.User.RequireUniqueEmail = false;
    o.SignIn.RequireConfirmedAccount = false;
    o.Lockout.MaxFailedAccessAttempts = 5;
    o.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
})
.AddEntityFrameworkStores<SmartLogContext>()
.AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(o =>
{
    o.Cookie.Name = "SmartReport.Auth";
    o.Cookie.HttpOnly = true;
    o.Cookie.SameSite = SameSiteMode.Lax;
    o.Cookie.SecurePolicy = CookieSecurePolicy.Always;

    o.ExpireTimeSpan = TimeSpan.FromDays(14);   // دو هفته
    o.SlidingExpiration = true;                 // با هر بازدید تمدید می‌شود

    o.LoginPath = "/login";
    o.LogoutPath = "/logout";
    o.AccessDeniedPath = "/access-denied";
});
var app = builder.Build();


using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<SmartLogContext>();
    db.Database.Migrate();

    await UserSeeder.SeedAsync(scope.ServiceProvider, app.Configuration);
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();
app.UseAuthentication();
app.UseAuthorization();
app.MapAuthEndpoints();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
