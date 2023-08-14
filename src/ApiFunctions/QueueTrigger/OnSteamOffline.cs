using Azure.Data.Tables;
using Azure.Storage.Queues;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Twitch.Net.Api.Client;
using Twitch.Net.EventSub.Notifications;
using TwitchLiveNotifications.Helpers;
using TwitchLiveNotifications.Models;

namespace TwitchLiveNotifications.EventFunctions;

public class OnStreamOffline
{
    private readonly ILogger _logger;
    private readonly TableClient _configTable;
    private readonly QueueServiceClient _queueClientService;
    private readonly IApiClient _apiClient;
    private readonly string _discordTemplate = Environment.GetEnvironmentVariable(Constants.DiscordTemplateOnStreamOffline);
    private readonly string _twitterTemplate = Environment.GetEnvironmentVariable(Constants.TwitterTemplateOnStreamOffline);

    public OnStreamOffline(ILoggerFactory loggerFactory, TableClient configTable, QueueServiceClient queueClientService, IApiClient apiClient)
    {
        _logger = loggerFactory.CreateLogger<OnStreamOffline>();
        _configTable = configTable;
        _queueClientService = queueClientService;
        _apiClient = apiClient;
    }

    [Function("OnStreamOffline")]
    public async Task<StreamOfflineNotificationEvent> Run([QueueTrigger("%queueEventOnStreamOffline%", Connection = "StorageQueueConnection")] StreamOfflineNotificationEvent notification)
    {
        var channelConfig = SubscriptionConfig.GetTwitchSubscriptionConfiguration(notification.BroadcasterIdString, _configTable);
        var streamStatus = StreamStatusEntry.GetTwitchStreamStatus(notification.BroadcasterIdString, _configTable);
        var streamUri = $"https://twitch.tv/{notification.BroadcasterUserName}";
        var gamename = streamStatus?.Game ?? "unknown";
        var title = streamStatus?.Title ?? "unknown";

        string streamerName;
        if (!string.IsNullOrEmpty(_twitterTemplate))
        {
            streamerName = string.IsNullOrEmpty(channelConfig.TwitterName) ? notification.BroadcasterUserName : $"@{channelConfig.TwitterName}";
            var tweetMessage = new TweetMessage()
            {
                Text = BuildMessage(_twitterTemplate, streamerName, streamUri, gamename, title)
            };
            QueueHelpers.SendMessage(_logger, _queueClientService, Constants.queueTwitterHandler, JsonSerializer.Serialize(tweetMessage));
        }

        if (!string.IsNullOrEmpty(_discordTemplate))
        {
            streamerName = string.IsNullOrEmpty(channelConfig.DiscordName) ? notification.BroadcasterUserName : $"<@{channelConfig.DiscordName}>";
            var discordMessage = new DiscordMessage()
            {
                Content = BuildMessage(_discordTemplate, streamerName, streamUri, gamename, title)
            };
            QueueHelpers.SendMessage(_logger, _queueClientService, Constants.queueDiscordHandler, JsonSerializer.Serialize(discordMessage));
        }

        if(streamStatus != null)
        {
            streamStatus.Status = StreamStatus.Offline;
            await StreamStatusEntry.SetTwitchStreamStatusAsync(streamStatus,_configTable);
        }

       return notification;
    }

    private static string BuildMessage(string template, string streamer, string streamuri, string gamename, string title)
    {
        return template
            .Replace("{streamer}", streamer, StringComparison.InvariantCultureIgnoreCase)
            .Replace("{streamuri}", streamuri, StringComparison.InvariantCultureIgnoreCase)
            .Replace("{gamename}", gamename, StringComparison.InvariantCultureIgnoreCase)
            .Replace("{title}", title, StringComparison.InvariantCultureIgnoreCase)
            .Replace("{newline}", Environment.NewLine, StringComparison.InvariantCultureIgnoreCase);
    }
}
