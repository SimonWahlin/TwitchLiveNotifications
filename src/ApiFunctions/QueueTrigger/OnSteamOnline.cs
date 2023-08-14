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

public class OnStreamOnline
{
    private readonly ILogger _logger;
    private readonly TableClient _configTable;
    private readonly QueueServiceClient _queueClientService;
    private readonly IApiClient _apiClient;
    private readonly string _discordTemplate = Environment.GetEnvironmentVariable(Constants.DiscordTemplateOnStreamOnline);
    private readonly string _twitterTemplate = Environment.GetEnvironmentVariable(Constants.TwitterTemplateOnStreamOnline);

    public OnStreamOnline(ILoggerFactory loggerFactory, TableClient configTable, QueueServiceClient queueClientService, IApiClient apiClient)
    {
        _logger = loggerFactory.CreateLogger<OnFollowed>();
        _configTable = configTable;
        _queueClientService = queueClientService;
        _apiClient = apiClient;
    }

    [Function("OnStreamOnline")]
    public async Task<StreamOnlineNotificationEvent> Run([QueueTrigger("%queueEventOnStreamOnline%", Connection = "StorageQueueConnection")] StreamOnlineNotificationEvent notification)
    {
        if (notification.Type != "live")
        {
            return notification;
        }

        var channelConfig = SubscriptionConfig.GetTwitchSubscriptionConfiguration(notification.BroadcasterIdString, _configTable);

        var streamUri = $"https://twitch.tv/{notification.BroadcasterUserName}";
        var channels = await _apiClient.Helix.Channels.GetChannelsAsync(
            ids: new List<string>
            {
                notification.BroadcasterIdString
            });
        var channel = channels.Channels.FirstOrDefault();
        if (channelConfig.ShouldSendNotification(channel.Title, channel.GameName))
        {

            string streamerName;

            if (!string.IsNullOrEmpty(_twitterTemplate))
            {
                streamerName = string.IsNullOrEmpty(channelConfig.TwitterName) ? notification.BroadcasterUserName : $"@{channelConfig.TwitterName}";
                var tweetMessage = new TweetMessage()
                {
                    Text = BuildMessage(_twitterTemplate, streamerName, streamUri, channel.GameName, channel.Title)
                };
                QueueHelpers.SendMessage(_logger, _queueClientService, Constants.queueTwitterHandler, JsonSerializer.Serialize(tweetMessage));

            }

            if (!string.IsNullOrEmpty(_discordTemplate))
            {
                streamerName = string.IsNullOrEmpty(channelConfig.DiscordName) ? notification.BroadcasterUserName : $"<@{channelConfig.DiscordName}>";
                var discordMessage = new DiscordMessage()
                {
                    Content = BuildMessage(_discordTemplate, streamerName, streamUri, channel.GameName, channel.Title)
                };
                QueueHelpers.SendMessage(_logger, _queueClientService, Constants.queueDiscordHandler, JsonSerializer.Serialize(discordMessage));
            }

            await StreamStatusEntry.SetTwitchStreamStatusAsync(
                new StreamStatusEntry()
                {
                    BroadCasterId = notification.BroadcasterIdString,
                    BoradCasterName = notification.BroadcasterUserName,
                    StreamUri = streamUri,
                    Game = channel.GameName,
                    Title = channel.Title,
                    StartedAt = notification.StartedAt,
                    Status = StreamStatus.Online
                },
                _configTable
            );
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
