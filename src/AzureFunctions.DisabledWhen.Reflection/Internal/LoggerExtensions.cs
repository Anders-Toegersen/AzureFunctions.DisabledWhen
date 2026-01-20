using Microsoft.Extensions.Logging;

namespace AzureFunctions.DisabledWhen.Internal;

internal static partial class LoggerExtensions
{
    [LoggerMessage(Level = LogLevel.Warning, EventName = nameof(FunctionDisabled), Message = "Function {FunctionName} is disabled")]
    public static partial void FunctionDisabled(this ILogger logger, string functionName);
}
