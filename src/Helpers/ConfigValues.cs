using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwitchLiveNotifications.Helpers
{
    internal class ConfigValues
    {
        // List of config values and which environment variable they are stored in.
        public static string DISABLE_NOTIFICATIONS = "DISABLE_NOTIFICATIONS";
        public static string DiscordOnFollowTemplate = "DiscordOnFollowTemplate";
        public static string DiscordOnStreamOnlineTemplate = "DiscordOnStreamOnlineTemplate";
        public static string DiscordWebhookUri = "DiscordWebhookUri";
        public static string queueAddSubscription = "queueAddSubscription";
        public static string queueDiscordHandler = "queueDiscordHandler";
        public static string queueEventOnFollow = "queueEventOnFollow";
        public static string queueEventOnSteamOnline = "queueEventOnSteamOnline";
        public static string queueEventOnStreamOffline = "queueEventOnStreamOffline";
        public static string queueTwitterHandler = "queueTwitterHandler";
        public static string Twitch_CallbackUrl = "Twitch_CallbackUrl";
        public static string Twitch_ClientId = "Twitch_ClientId";
        public static string Twitch_ClientSecret = "Twitch_ClientSecret";
        public static string Twitch_SignatureSecret = "Twitch_SignatureSecret";
        public static string TwitterConsumerKey = "TwitterConsumerKey";
        public static string TwitterConsumerSecret = "TwitterConsumerSecret";
        public static string TwitterAccessToken = "TwitterAccessToken";
        public static string TwitterAccessTokenSecret = "TwitterAccessTokenSecret";
        public static string TwitterOnFollowTemplate = "TwitterOnFollowTemplate";
        public static string TwitterOnStreamOnlineTemplate = "TwitterOnStreamOnlineTemplate";
        public static string tableTwichLiveNotificationsConfiguration = "TwichLiveNotificationsConfiguration";
        public static string QueueServiceStorageAccount = "AzureWebJobsStorage";
        public static string TableServiceStorageAccount = "AzureWebJobsStorage";
    }
}
