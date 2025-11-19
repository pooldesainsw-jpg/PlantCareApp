using System.IO;
using Microsoft.EntityFrameworkCore;
using PlantCareApp.Components;
using PlantCareApp.Data;
using PlantCareApp.Options;
using PlantCareApp.Services;

var builder = WebApplication.CreateBuilder(args);

var configuredDbPath = Environment.GetEnvironmentVariable("PLANTAPP_DB_PATH");
string databasePath;

if (string.IsNullOrWhiteSpace(configuredDbPath))
{
    var dataDirectory = Path.Combine(AppContext.BaseDirectory, "data");
    Directory.CreateDirectory(dataDirectory);
    databasePath = Path.Combine(dataDirectory, "plants.db");
}
else
{
    var directory = Path.GetDirectoryName(configuredDbPath);
    if (!string.IsNullOrEmpty(directory))
    {
        Directory.CreateDirectory(directory);
    }

    databasePath = configuredDbPath;
}

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite($"Data Source={databasePath}"));

builder.Services.Configure<OpenAIOptions>(builder.Configuration.GetSection("OpenAI"));

builder.Services.AddScoped<PlantService>();
builder.Services.AddScoped<ImageStorageService>();
builder.Services.AddHttpClient<WeatherService>();
builder.Services.AddScoped<ChatService>();
builder.Services.AddScoped<PhotoAnalysisService>();
builder.Services.AddScoped<RecommendationService>();
builder.Services.AddScoped<PlantTimelineService>();
builder.Services.AddScoped<CareAutomationService>();
builder.Services.AddHttpClient<PlantIdentificationService>();
builder.Services.AddHttpClient<LocationLookupService>();
builder.Services.AddSingleton<HomeZoneService>();
builder.Services.AddSingleton<UserSettingsService>();

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
