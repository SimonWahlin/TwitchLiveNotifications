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

public class ResolveUserId
{
    private readonly ILogger _logger;
    private readonly IApiClient _apiClient;

    public ResolveUserId(ILoggerFactory loggerFactory, IApiClient apiClient)
    {
        _logger = loggerFactory.CreateLogger<ResolveUserId>();
        _apiClient = apiClient;
    }

    [Function("ResolveUserId")]
    public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequestData req)
    {
        var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
        _logger.LogInformation("Request body: {RequestBody}", requestBody);

        var usernames = JsonSerializer.Deserialize<List<string>>(requestBody);
        _logger.LogInformation("Usernames: {Usernames}", string.Join(',', usernames));

        var users = await _apiClient.Helix.Users.GetUsersAsync(logins: usernames);

        var options = new JsonSerializerOptions { WriteIndented = true };
        HttpResponseData response;
        if (users.Successfully == 1)
        {
            response = req.CreateResponse(HttpStatusCode.OK);
            response.Headers.Add("Content-Type", "application/json; charset=utf-8");
            response.WriteString(JsonSerializer.Serialize(users.Users, options));
            return response;
        }
        response = req.CreateResponse(HttpStatusCode.NotFound);
        return response;
    }
}
