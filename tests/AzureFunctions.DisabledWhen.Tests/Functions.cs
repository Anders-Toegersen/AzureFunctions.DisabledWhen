using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;

namespace AzureFunctions.DisabledWhen.Tests;

public class Functions
{
    [Function("AlwaysEnabled")]
    public HttpResponseData AlwaysEnabled(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequestData req)
        => req.CreateResponse(System.Net.HttpStatusCode.OK);

    [Function("DisabledInProduction")]
    [DisabledWhen("ENVIRONMENT", "Production")]
    public HttpResponseData DisabledInProduction(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequestData req)
        => req.CreateResponse(System.Net.HttpStatusCode.OK);

    [Function("FeatureFlagged")]
    [DisabledWhen("FEATURE_FLAG", "disabled", StringComparison.OrdinalIgnoreCase)]
    public HttpResponseData FeatureFlagged(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequestData req)
        => req.CreateResponse(System.Net.HttpStatusCode.OK);

    [Function("DisabledLocally")]
    [DisabledWhenLocal]
    public HttpResponseData DisabledLocally(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequestData req)
        => req.CreateResponse(System.Net.HttpStatusCode.OK);

    [Function("RequiresApiKey")]
    [DisabledWhenNullOrEmpty("API_KEY")]
    public HttpResponseData RequiresApiKey(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequestData req)
        => req.CreateResponse(System.Net.HttpStatusCode.OK);

    [Function("MultipleConditions")]
    [DisabledWhen("ENVIRONMENT", "Production")]
    [DisabledWhen("DEBUG_MODE", "true")]
    public HttpResponseData MultipleConditions(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequestData req)
        => req.CreateResponse(System.Net.HttpStatusCode.OK);

    [Function("MixedConditions")]
    [DisabledWhenLocal]
    [DisabledWhenNullOrEmpty("CONNECTION_STRING")]
    public HttpResponseData MixedConditions(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequestData req)
        => req.CreateResponse(System.Net.HttpStatusCode.OK);

    [Function("NestedConfigDisabled")]
    [DisabledWhen("Features:BetaFeature:Enabled", "false")]
    public HttpResponseData NestedConfigDisabled(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequestData req)
        => req.CreateResponse(System.Net.HttpStatusCode.OK);

    [Function("NestedConfigRequired")]
    [DisabledWhenNullOrEmpty("Database:ConnectionString")]
    public HttpResponseData NestedConfigRequired(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequestData req)
        => req.CreateResponse(System.Net.HttpStatusCode.OK);
}
