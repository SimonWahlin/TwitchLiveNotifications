using Azure.Data.Tables;
using Azure.Storage.Queues;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Optional.Unsafe;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using Twitch.Net.Api.Client;
using Twitch.Net.EventSub;
using TwitchLiveNotifications.Helpers;
using TwitchLiveNotifications.Models;

namespace TwitchLiveNotifications.EventSubFunctions;

public class ClearSubscriptions
{
    private readonly ILogger _logger;
    private readonly IApiClient _apiClient;
    private readonly TableClient _configTable;
    private readonly QueueServiceClient _queueClientService;
    private readonly IEventSubService2 _eventSubService;
    private readonly EventSubBuilder _eventSubBuilder;

    public ClearSubscriptions(ILoggerFactory loggerFactory, IApiClient apiClient, TableClient configTable, QueueServiceClient queueClientService, IEventSubService2 eventSubService, EventSubBuilder subBuilder)
    {
        _logger = loggerFactory.CreateLogger<ClearSubscriptions>();
        _apiClient = apiClient;
        _configTable = configTable;
        _queueClientService = queueClientService;
        _eventSubService = eventSubService;
        _eventSubBuilder = subBuilder;
    }

    [Function("ClearSubscriptions")]
    public async Task<HttpResponseData> RunAsync([HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequestData req)
    {

        _logger.LogInformation("######################################################################");
        _logger.LogInformation("#######################################################################");
        _logger.LogInformation("########################################################################");
        _logger.LogInformation("#########################################################################");
        _logger.LogInformation("ConfigValue: Twitch_ClientId = {value}", Environment.GetEnvironmentVariable(Constants.TwitchClientId));
        _logger.LogInformation("ConfigValue: Twitch_ClientSecret = {value}", Environment.GetEnvironmentVariable(Constants.TwitchClientSecret));
        _logger.LogInformation("ConfigValue: Twitch_CallbackUrl = {value}", Environment.GetEnvironmentVariable(Constants.TwitchCallbackUrl));
        _logger.LogInformation("ConfigValue: Twitch_SignatureSecret = {value}", Environment.GetEnvironmentVariable(Constants.TwitchSignatureSecret));

        HttpResponseData response;

        var subscriptions = await _eventSubService.Subscriptions();

        foreach (var sub in subscriptions.ValueOrDefault().Data)
        {
            QueueHelpers.SendMessage(_logger, _queueClientService, "queueRemoveSubscription", sub.Id);
        }
        response = req.CreateResponse(HttpStatusCode.OK);
        response.Headers.Add("Content-Type", "text/plain; charset=utf-8");

        response.WriteString("All subscripitions removed.");

        return response;
    }
}
