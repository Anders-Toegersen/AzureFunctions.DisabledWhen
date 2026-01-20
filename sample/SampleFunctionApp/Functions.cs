using AzureFunctions.DisabledWhen;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;

namespace SampleFunctionApp;

public static class Functions
{
    [Function("ScheduledCleanup")]
    [DisabledWhenLocal]
    public static void ScheduledCleanup(
        [TimerTrigger("0 */5 * * * *", RunOnStartup = true)] TimerInfo timer)
    {
        Console.WriteLine($"Cleanup ran at: {DateTime.UtcNow:O}");
    }

    [Function("ProcessOrderQueue")]
    [DisabledWhenNullOrEmpty("ServiceBusConnection")]
    public static void ProcessOrderQueue(
        [ServiceBusTrigger("orders", Connection = "ServiceBusConnection")] string message)
    {
        Console.WriteLine($"Processing order: {message}");
    }

    [Function("GdprDataExport")]
    [DisabledWhen("EnvironmentOptions:RegionAbbreviation", "US", StringComparison.OrdinalIgnoreCase)]
    [DisabledWhen("EnvironmentOptions:RegionAbbreviation", "CA", StringComparison.OrdinalIgnoreCase)]
    [DisabledWhen("EnvironmentOptions:RegionAbbreviation", "AU", StringComparison.OrdinalIgnoreCase)]
    public static IActionResult GdprDataExport(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "gdpr/export/{userId}")] HttpRequest req,
        string userId)
    {
        return new OkObjectResult($"GDPR data export for user {userId}");
    }
}
