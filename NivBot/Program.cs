using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NetCord;
using NetCord.Gateway;
using NetCord.Hosting.Gateway;
using NetCord.Hosting.Services.ApplicationCommands;
using NetCord.Logging;
using NivBot.DataLayer;
using NivBot.ExternalServicesLayer.OsrsAPI;
using NivBot.Netcord.Modules;
using System.ComponentModel;
using System.Text.Json;

// Do all of the DI
HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);

// Add the DI for the OsrsAPI
builder.Services
    .AddHttpClient<IOsrsHighscoreService, OsrsHighscoreService>(client => {
        client.BaseAddress = new Uri("https://secure.runescape.com/m=hiscore_oldschool/");
    });
// Add the DI for Netcord
builder.Services
    .AddDiscordGateway()
    .AddApplicationCommands();

// Add the DI for the DbContext
builder.Services
    .AddDataLayer(builder.Configuration, true);

// Testing more DI
builder.Services.AddScoped<LinkOsrsAccountService>();

IHost app = builder.Build();

//using (IServiceScope scope = app.Services.CreateScope())
//{
//    var hs = app.Services.GetRequiredService<IOsrsHighscoreService>();
//    var stats = await hs.GetPlayerStatsAsync("Ni123v Lem");
//    Console.WriteLine(JsonSerializer.Serialize(stats, new JsonSerializerOptions { WriteIndented = true }));
//}

await app.RunAsync();
