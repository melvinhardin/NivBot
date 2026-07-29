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
using NivBot.Features.LinkOsrsAccount;
using NivBot.Features.RegisterGoodplaceUser;
using NivBot.Features.SyncActivities;
using System.ComponentModel;
using System.Text.Json;
// testing


// Do all of the DI
HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);

// Add the DI for the OsrsAPI
builder.Services
    .AddHttpClient<IOsrsHighscoreService, OsrsHighscoreService>(client =>
    {
        client.BaseAddress = new Uri("https://secure.runescape.com/m=hiscore_oldschool/");
    });
// Add the DI for Netcord

builder.Services
    .AddDiscordGateway()
    .AddApplicationCommands();

// Add the DI for the DbContext
builder.Services
    .AddDataLayer(builder.Configuration, true);

// Adding slash commands DI.2
builder.Services.AddScoped<LinkOsrsAccountService>();
builder.Services.AddScoped<RegisterGoodplaceUserService>();
builder.Services.AddScoped<SyncActivitiesService>();

IHost app = builder.Build();

// Add the application commands
app.AddModules(typeof(Program).Assembly);



await app.RunAsync();
