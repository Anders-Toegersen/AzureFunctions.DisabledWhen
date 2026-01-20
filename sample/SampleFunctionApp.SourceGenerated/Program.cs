using AzureFunctions.DisabledWhen;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var host = new HostBuilder()
    .ConfigureFunctionsWebApplication()
    .UseDisabledWhen()
    .ConfigureLogging(logging => logging.AddConsole())
    .Build();

host.Run();
