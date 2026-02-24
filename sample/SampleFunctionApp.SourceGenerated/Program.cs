using AzureFunctions.DisabledWhen;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var builder = FunctionsApplication.CreateBuilder(args);
builder.UseDisabledWhen();
builder.ConfigureLogging(logging => logging.AddConsole());

var host = builder.Build();
host.Run();
