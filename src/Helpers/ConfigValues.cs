namespace TwitchLiveNotifications.Helpers;

internal class ConfigValues
{
    // List of config values and which environment variable they are stored in.
    public static string DISABLE_NOTIFICATIONS = "DISABLE_NOTIFICATIONS";
    public static string DiscordTemplateOnFollow = "DiscordTemplateOnFollow";
    public static string DiscordTemplateOnStreamOnline = "DiscordTemplateOnStreamOnline";
    public static string DiscordWebhookUri = "DiscordWebhookUri";
    public static string queueAddSubscription = "queueAddSubscription";
    public static string queueRemoveSubscription = "queueRemoveSubscription";
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
    public static string TwitterUseV2API = "TwitterUseV2API";
    public static string TwitterTemplateOnFollow = "TwitterTemplateOnFollow";
    public static string TwitterTemplateOnStreamOnline = "TwitterTemplateOnStreamOnline";
    public static string tableTwichLiveNotificationsConfiguration = "TwichLiveNotificationsConfiguration";
    public static string QueueServiceStorageAccount = "StorageQueueConnection__queueServiceUri";
    public static string TableServiceStorageAccount = "StorageTableConnection__tableServiceUri";
    public static string TwitterUri = "https://api.twitter.com/2/tweets";
}
