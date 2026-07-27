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
using System.ComponentModel;
using System.Text.Json;


HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);
builder.Services
    .AddHttpClient<IOsrsHighscoreService, OsrsHighscoreService>(client => {
        client.BaseAddress = new Uri("https://secure.runescape.com/m=hiscore_oldschool/");
    });
builder.Services
    .AddDiscordGateway()
    .AddApplicationCommands();
builder.Services
    .AddDataLayer(builder.Configuration, true);

IHost app = builder.Build();

//using (IServiceScope scope = app.Services.CreateScope())
//{
//    var hs = app.Services.GetRequiredService<IOsrsHighscoreService>();
//    var stats = await hs.GetPlayerStatsAsync("Ni123v Lem");
//    Console.WriteLine(JsonSerializer.Serialize(stats, new JsonSerializerOptions { WriteIndented = true }));
//}

await app.RunAsync();
