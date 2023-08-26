using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using Twitch.Net.Api.Client;

namespace TwitchLiveNotifications.ApiFunctions;

public class ResolveChannelId
{
    private readonly ILogger _logger;
    private readonly IApiClient _apiClient;

    public ResolveChannelId(ILoggerFactory loggerFactory, IApiClient apiClient)
    {
        _logger = loggerFactory.CreateLogger<ResolveChannelId>();
        _apiClient = apiClient;
    }

    [Function("ResolveChannelId")]
    public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequestData req)
    {
        var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
        _logger.LogInformation("Request body: {RequestBody}", requestBody);

        var ids = JsonSerializer.Deserialize<List<string>>(requestBody);
        _logger.LogInformation("ids: {Usernames}", string.Join(',', ids));

        var channels = await _apiClient.Helix.Channels.GetChannelsAsync(ids: ids);

        var options = new JsonSerializerOptions { WriteIndented = true };
        HttpResponseData response;
        if (channels.Successfully == 1)
        {
            response = req.CreateResponse(HttpStatusCode.OK);
            response.Headers.Add("Content-Type", "application/json; charset=utf-8");
            response.WriteString(JsonSerializer.Serialize(channels.Channels, options));
            return response;
        }
        response = req.CreateResponse(HttpStatusCode.NotFound);
        return response;
    }
}
