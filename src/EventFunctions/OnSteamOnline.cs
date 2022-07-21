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

namespace TwitchLiveNotifications.EventFunctions
{
    public class OnStreamOnline
    {
        private readonly ILogger _logger;
        private readonly TableClient _configTable;
        private readonly QueueServiceClient _queueClientService;
        private readonly IApiClient _apiClient;
        private readonly string _discordTemplate = Environment.GetEnvironmentVariable(ConfigValues.DiscordOnStreamOnlineTemplate);
        private readonly string _twitterTemplate = Environment.GetEnvironmentVariable(ConfigValues.TwitterOnStreamOnlineTemplate);

        public OnStreamOnline(ILoggerFactory loggerFactory, TableClient configTable, QueueServiceClient queueClientService, IApiClient apiClient)
        {
            _logger = loggerFactory.CreateLogger<OnFollowed>();
            _configTable = configTable;
            _queueClientService = queueClientService;
            _apiClient = apiClient;
        }

        [Function("OnStreamOnline")]
        public async Task<StreamOnlineNotificationEvent> Run([QueueTrigger("%queueEventOnSteamOnline%", Connection = "StorageQueueConnection")] StreamOnlineNotificationEvent onlineNotification)
        {
            if (onlineNotification.Type != "live")
            {
                return onlineNotification;
            }

            var channelConfig = SubscriptionConfig.GetTwitchSubscriptionConfiguration(onlineNotification.BroadcasterIdString, _configTable);
            var streamUri = $"https://twitch.tv/{onlineNotification.BroadcasterUserName}";
            var channels = await _apiClient.Helix.Channels.GetChannelsAsync(
                ids: new List<string>
                {
                    onlineNotification.BroadcasterIdString
                });
            var channel = channels.Channels.FirstOrDefault();

            var streamerName = string.IsNullOrEmpty(channelConfig.TwitterName) ? onlineNotification.BroadcasterUserName : $"@{channelConfig.TwitterName}";
            var tweetMessage = new TweetMessage()
            {
                Text = BuildMessage(_twitterTemplate, streamerName, streamUri, channel.GameName, channel.Title)
            };
            QueueHelpers.SendMessage(_logger, _queueClientService, ConfigValues.queueTwitterHandler, JsonSerializer.Serialize(tweetMessage));

            streamerName = string.IsNullOrEmpty(channelConfig.DiscordName) ? onlineNotification.BroadcasterUserName : $"<@{channelConfig.DiscordName}>";
            var discordMessage = new DiscordMessage()
            {
                Content = BuildMessage(_discordTemplate, streamerName, streamUri, channel.GameName, channel.Title)
            };
            QueueHelpers.SendMessage(_logger, _queueClientService, ConfigValues.queueDiscordHandler, JsonSerializer.Serialize(discordMessage));
            return onlineNotification;
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
}
