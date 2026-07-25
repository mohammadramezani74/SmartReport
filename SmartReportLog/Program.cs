using Microsoft.EntityFrameworkCore;
using SmartReportLog.Components;
using SmartReportLog.Persistance;
using SmartReportLog.Services.atm.Command.SaveData;
using SmartReportLog.Services.atm.Query;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddDbContext<SmartLogContext>(x =>
{
    x.UseSqlServer(builder.Configuration.GetConnectionString("ApplicationDbContext")).EnableSensitiveDataLogging().LogTo(Console.WriteLine, LogLevel.Information); ;
});
builder.Services.AddScoped<IAtmIngestionService,AtmIngestionService>();
builder.Services.AddScoped<IAtmQueryService, AtmQueryService>();
var app = builder.Build();


using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<SmartLogContext>();
    db.Database.Migrate();
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
