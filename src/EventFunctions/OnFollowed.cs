using Azure.Data.Tables;
using Azure.Storage.Queues;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System;
using Twitch.Net.EventSub.Notifications;
using TwitchLiveNotifications.Helpers;
using TwitchLiveNotifications.Models;

namespace TwitchLiveNotifications.EventFunctions
{
    public class OnFollowed
    {
        private readonly ILogger _logger;
        private readonly TableClient _configTable;
        private readonly QueueServiceClient _queueClientService;
        private readonly string _discordTemplate = Environment.GetEnvironmentVariable(ConfigValues.DiscordOnFollowTemplate);
        private readonly string _twitterTemplate = Environment.GetEnvironmentVariable(ConfigValues.TwitterOnFollowTemplate);

        public OnFollowed(ILoggerFactory loggerFactory, TableClient configTable, QueueServiceClient queueClientService)
        {
            _logger = loggerFactory.CreateLogger<OnFollowed>();
            _configTable = configTable;
            _queueClientService = queueClientService;
        }

        [Function("OnFollowed")]
        public void Run([QueueTrigger("%queueEventOnFollow%", Connection = "")] ChannelFollowNotificationEvent followEvent)
        {
            _logger.LogInformation("Got follow event: {event}",JsonSerializer.Serialize(followEvent, new JsonSerializerOptions { WriteIndented = true }));
            var channelConfig = SubscriptionConfig.GetTwitchSubscriptionConfiguration(followEvent.BroadcasterIdString, _configTable);
            var streamUri = $"https://twitch.tv/{followEvent.BroadcasterUserName}";

            string streamerName;
            if(channelConfig == null || string.IsNullOrEmpty(channelConfig.TwitterName))
            {
                streamerName = followEvent.BroadcasterUserName;
            }
            else
            {
                streamerName = $"@{channelConfig.TwitterName}";
            }
            string tweet = _twitterTemplate
                .Replace("{follower}", followEvent.UserName)
                .Replace("{streamer}", streamerName)
                .Replace("{followedat}", followEvent.FollowedAt.ToString())
                .Replace("{streamuri}", streamUri)
                .Replace("{newline}", Environment.NewLine);
            QueueHelpers.SendMessage(_logger, _queueClientService, "queueTwitterHandler", tweet);

            if (channelConfig == null || string.IsNullOrEmpty(channelConfig.DiscordName))
            {
                streamerName = followEvent.BroadcasterUserName;
            }
            else
            {
                streamerName = $"@{channelConfig.DiscordName}";
            }
            var discordMessage = new DiscordMessage()
            {
                Content = _discordTemplate
                    .Replace("{follower}", followEvent.UserName)
                    .Replace("{streamer}", streamerName)
                    .Replace("{followedat}", followEvent.FollowedAt.ToString())
                    .Replace("{streamuri}", streamUri)
                    .Replace("{newline}", Environment.NewLine)
            };
            QueueHelpers.SendMessage(_logger, _queueClientService, "queueDiscordHandler", JsonSerializer.Serialize(discordMessage));
        }
    }
}
