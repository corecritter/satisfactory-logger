using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SatisfactoryLogger;

var builder = new ConfigurationBuilder();
builder.SetBasePath(Directory.GetCurrentDirectory())
       .AddJsonFile("appSettings.json", optional: false, reloadOnChange: false);
IConfigurationRoot configuration = builder.Build();
var appSettings = new AppSettings();
configuration.Bind(appSettings);

var sp = new ServiceCollection()
    .AddLogging(_ =>
    {
        _.ClearProviders();
        //_.AddConsole();
        _.AddSimpleConsole(config =>
        {
            config.TimestampFormat = "[MM dd yyyy HH:mm:ss] ";
            config.UseUtcTimestamp = false;
        });
    })
    .AddSingleton(appSettings)
    .Build()
    .BuildServiceProvider();

var ts = new CancellationTokenSource();
var loggerService = sp.GetRequiredService<LoggerService>();

await loggerService.Run(ts.Token);
