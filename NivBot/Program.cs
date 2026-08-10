using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NetCord;
using NetCord.Gateway;
using NetCord.Hosting.Gateway;
using NetCord.Hosting.Services;
using NetCord.Hosting.Services.ApplicationCommands;
using NetCord.Logging;
using NivBot.DataLayer;
using NivBot.ExternalServicesLayer.OsrsAPI;
using NivBot.ExternalServicesLayer.TempleAPI;
using NivBot.Features.LinkOsrsAccount;
using NivBot.Features.RegisterGoodplaceUser;
using NivBot.Features.SyncActivities;
using System.ComponentModel;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore.Design;
using NivBot.Features.SyncCollectionList;
using NivBot.Features.SyncUserCollections;
// testing


// Do all of the DI
HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);
Console.WriteLine(builder.Configuration.GetConnectionString("Goodplace") ?? "<null>");
Console.WriteLine(builder.Environment.ContentRootPath);
// Add the DI for the OsrsAPI
builder.Services
    .AddHttpClient<IOsrsHighscoreService, OsrsHighscoreService>(client =>
    {
        client.BaseAddress = new Uri("https://secure.runescape.com/m=hiscore_oldschool/");
    });
// Add the DI for the TempleAPI
builder.Services
    .AddHttpClient<ITempleService, TempleService>(client =>
    {
        client.BaseAddress = new Uri("https://templeosrs.com/api/");
    });


//// Add the DI for Netcord

//builder.Services
//    .AddDiscordGateway()
//    .AddApplicationCommands();

// Add the DI for the DbContext

builder.Services
    .AddDataLayer(builder.Configuration, true);

//// Adding slash commands DI.2
builder.Services.AddScoped<LinkOsrsAccountService>();
builder.Services.AddScoped<RegisterGoodplaceUserService>();
builder.Services.AddScoped<SyncActivitiesService>();
builder.Services.AddScoped<SyncCollectionListService>();
builder.Services.AddScoped<SyncOsrsAccountCollectionService>();
IHost app = builder.Build();
var createAccount = app.Services.GetRequiredService<RegisterGoodplaceUserService>();
var test = app.Services.GetRequiredService<SyncActivitiesService>();
var test2 = app.Services.GetRequiredService<LinkOsrsAccountService>();
var test3 = app.Services.GetRequiredService<SyncCollectionListService>();
var test4 = app.Services.GetRequiredService<SyncOsrsAccountCollectionService>();

await test4.SyncGroupAccountCollog(948);

//var options = new JsonSerializerOptions
//{
//    WriteIndented = true,
//    ReferenceHandler = ReferenceHandler.IgnoreCycles  // drop if not EF entities
//};

//var path = Path.Combine(AppContext.BaseDirectory, "models.json");
//File.WriteAllText(path, JsonSerializer.Serialize(test, options));
//Console.WriteLine(path);   // print it so you can paste into the browser
//// Add the application commands
//app.AddModules(typeof(Program).Assembly);



await app.RunAsync();
