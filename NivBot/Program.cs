using Microsoft.Extensions.Configuration;
using NetCord;
using NetCord.Gateway;
using NetCord.Logging;
using System.ComponentModel;
IConfigurationRoot config = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .AddEnvironmentVariables()
    .Build();
string discKey = config.GetRequiredSection("Discord").GetRequiredSection("Token").Get<string>();

GatewayClient client = new(new BotToken(discKey), new GatewayClientConfiguration
{
    Logger = new ConsoleLogger(),
});

await client.StartAsync();
await Task.Delay(-1);