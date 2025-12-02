using GmailInvoiceAnalyzer;
using Microsoft.Extensions.Configuration;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .WriteTo.Console()
    .WriteTo.File("logs/GmailInvoiceAnalyzer.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

var config = new ConfigurationBuilder()
                 .SetBasePath(Directory.GetCurrentDirectory())
                 .AddJsonFile("config\\appsettings.json")
                 .Build();

var commonSettingsSection = config.GetSection("CommonSettings");

Manager manager = new Manager(commonSettingsSection);

try
{
    await manager.Execute();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}