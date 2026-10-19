using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NetCord;
using NetCord.Hosting.Gateway;
using NetCord.Hosting.Services;
using NetCord.Hosting.Services.ApplicationCommands;
using NetCord.Hosting.Services.ComponentInteractions;
using NetCord.Services.ComponentInteractions;
using NivBot.DataLayer;
using NivBot.ExternalServicesLayer.OsrsAPI;
using NivBot.ExternalServicesLayer.TempleAPI;
using NivBot.Features.GoodplaceTask;
using NivBot.Features.LinkOsrsAccount;
using NivBot.Features.RegisterGoodplaceUser;
using NivBot.Features.Shared;
using NivBot.Features.SyncActivities;
using NivBot.Features.SyncCollectionList;
using NivBot.Features.SyncUserCollections;

// Configuration comes from appsettings.json (see appsettings.example.json) and environment variables,
// e.g. Discord__Token and ConnectionStrings__Goodplace in docker compose.
HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);

builder.Services.AddSingleton(builder.Configuration.GetSection("Tasks").Get<TaskOptions>() ?? new TaskOptions());
builder.Services.AddSingleton(builder.Configuration.GetSection("Osrs").Get<OsrsOptions>() ?? new OsrsOptions());

// Add the DI for the OsrsAPI
builder.Services
    .AddHttpClient<IOsrsHighscoreService, OsrsHighscoreService>(client =>
    {
        client.BaseAddress = new Uri("https://secure.runescape.com/m=hiscore_oldschool/");
        client.Timeout = TimeSpan.FromSeconds(20);
    });
// Add the DI for the TempleAPI
builder.Services
    .AddHttpClient<ITempleService, TempleService>(client =>
    {
        client.BaseAddress = new Uri("https://templeosrs.com/api/");
    });

// Add the DI for Netcord: slash commands, and the buttons / select menus used by the task commands
builder.Services
    .AddDiscordGateway()
    .AddApplicationCommands()
    .AddComponentInteractions<ButtonInteraction, ButtonInteractionContext>()
    .AddComponentInteractions<StringMenuInteraction, StringMenuInteractionContext>();

// Add the DI for the DbContext, sensitive data (parameter values) is only logged when running as Development
builder.Services
    .AddDataLayer(builder.Configuration, builder.Environment.IsDevelopment());

// Feature services
builder.Services.AddScoped<OsrsAccountStatsService>();
builder.Services.AddScoped<LinkOsrsAccountService>();
builder.Services.AddScoped<RegisterGoodplaceUserService>();
builder.Services.AddScoped<SyncActivitiesService>();
builder.Services.AddScoped<SyncCollectionListService>();
builder.Services.AddScoped<SyncOsrsAccountCollectionService>();
builder.Services.AddScoped<GoodplaceTaskService>();
builder.Services.AddSingleton<TaskGoalCalculator>();

IHost app = builder.Build();

// Apply pending migrations on startup, so a fresh database (e.g. in docker compose) is created automatically
using (IServiceScope scope = app.Services.CreateScope())
{
    GoodplaceContext db = scope.ServiceProvider.GetRequiredService<GoodplaceContext>();
    await db.Database.MigrateAsync();
}

// Add the application commands and component interactions
app.AddModules(typeof(Program).Assembly);

await app.RunAsync();
