using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;

namespace TwitchLiveNotifications.EventSubFunctions;

public class GetSubscriptions
{
    private readonly IEventSubService2 _eventSubService;

    public GetSubscriptions(IEventSubService2 eventSubService)
    {
        _eventSubService = eventSubService;
    }

    [Function("GetSubscriptions")]
    public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Function, "get")] HttpRequestData req)
    {
        var result = await _eventSubService.Subscriptions();
        HttpResponseData response;
        if (!result.HasValue)
        {
            response = req.CreateResponse(HttpStatusCode.BadRequest);
            return response;
        }

        response = req.CreateResponse(HttpStatusCode.OK);
        response.Headers.Add("Content-Type", "application/json; charset=utf-8");

        response.WriteString(JsonSerializer.Serialize(
            result.ValueOrDefault(),
            new JsonSerializerOptions { WriteIndented = true }
        ));

        return response;
    }
}
