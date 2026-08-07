using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;
using SmartReportLog.Components;
using SmartReportLog.Endpoints;
using SmartReportLog.Entity.Identity;
using SmartReportLog.Persistance;
using SmartReportLog.Services.atm.Command.SaveData;
using SmartReportLog.Services.atm.Query;
using SmartReportLog.Services.Auth;
using SmartReportLog.Services.Sana;
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
builder.Services.Configure<SanaStorageOptions>(
    builder.Configuration.GetSection(SanaStorageOptions.SectionName));

builder.Services.AddScoped<ISanaArchiveService, SanaArchiveService>();

builder.Services.Configure<FormOptions>(o =>
{
    o.MultipartBodyLengthLimit = 209715200; // ۲۰۰ مگابایت
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(o =>
{
    o.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "SmartReport API",
        Version = "v1",
        Description = "سرویس‌های دریافت گزارش توتال از سانا و بررسی وضعیت تیکت"
    });

    o.AddSecurityDefinition("ApiKey", new OpenApiSecurityScheme
    {
        Name = "X-Api-Key",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Description = "کلید دسترسی سرویس. مقدار را در هدر X-Api-Key ارسال کنید."
    });
});
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
app.UseSwagger();
app.UseSwaggerUI(o =>
{
    o.SwaggerEndpoint("/swagger/v1/swagger.json", "SmartReport API v1");
    o.RoutePrefix = "api-docs";
    o.DocumentTitle = "مستندات SmartReport";
});
app.MapAuthEndpoints();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
