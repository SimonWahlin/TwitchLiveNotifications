using Azure.Data.Tables;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Text.Json;
using TwitchLiveNotifications.Models;

namespace TwitchLiveNotifications.ApiFunctions;

public class GetOnlineChannels
{
    private readonly ILogger _logger;
    private readonly TableClient _configTable;

    public GetOnlineChannels(ILoggerFactory loggerFactory, TableClient configTable)
    {
        _logger = loggerFactory.CreateLogger<GetOnlineChannels>();
        _configTable = configTable;
    }

    [Function("GetOnlineChannels")]
    public HttpResponseData Run([HttpTrigger(AuthorizationLevel.Function, "get")] HttpRequestData req)
    {
        var channels = StreamStatusEntry.GetTwitchStreamStatus(_configTable);

        var options = new JsonSerializerOptions { WriteIndented = true };
        HttpResponseData response;
        response = req.CreateResponse(HttpStatusCode.OK);
        response.Headers.Add("Content-Type", "application/json; charset=utf-8");
        response.WriteString(JsonSerializer.Serialize(channels, options));
        return response;
    }
}
