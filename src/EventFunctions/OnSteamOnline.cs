using Azure.Data.Tables;
using Azure.Storage.Queues;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System;
using System.Text.Json;
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
        public void Run([QueueTrigger("%queueEventOnSteamOnline%", Connection = "")] StreamOnlineNotificationEvent onlineNotification)
        {
            if (onlineNotification.Type != "live")
            {
                return;
            }

            var channelConfig = SubscriptionConfig.GetTwitchSubscriptionConfiguration(onlineNotification.BroadcasterIdString, _configTable);
            var streamUri = $"https://twitch.tv/{onlineNotification.BroadcasterUserName}";
            //var twitchChannelRequest = await _apiClient.Helix.Channels.GetUsersAsync(
            //    logins: new List<string>
            //    {
            //        onlineNotification.BroadcasterIdString
            //    });
            //string category;
            // TODO: Fix Channels support for APIClient so we can lookup game and title

            var streamerName = string.IsNullOrEmpty(channelConfig.TwitterName) ? onlineNotification.BroadcasterUserName : $"@{channelConfig.TwitterName}";
            string tweet = _twitterTemplate
                .Replace("{streamer}", streamerName)
                .Replace("{streamuri}", streamUri)
                .Replace("{newline}", Environment.NewLine);
            QueueHelpers.SendMessage(_logger, _queueClientService, "queueTwitterHandler", tweet);


            streamerName = string.IsNullOrEmpty(channelConfig.DiscordName) ? onlineNotification.BroadcasterUserName : $"@{channelConfig.DiscordName}";
            var discordMessage = new DiscordMessage()
            {
                Content = _discordTemplate
                    .Replace("{streamer}", streamerName)
                    .Replace("{streamuri}", streamUri)
                    .Replace("{newline}", Environment.NewLine)
            };
            QueueHelpers.SendMessage(_logger, _queueClientService, "queueDiscordHandler", JsonSerializer.Serialize(discordMessage));
            return;
        }
    }
}
