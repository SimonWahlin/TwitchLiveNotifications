namespace TwitchLiveNotifications.Helpers;

internal static class Constants
{
    // List of config values and which environment variable they are stored in.
    public static readonly string DisableNotifications = "DISABLE_NOTIFICATIONS";
    public static readonly string DiscordTemplateOnFollow = "DiscordTemplateOnFollow";
    public static readonly string DiscordTemplateOnStreamOnline = "DiscordTemplateOnStreamOnline";
    public static readonly string DiscordTemplateOnStreamOffline = "DiscordTemplateOnStreamOffline";
    public static readonly string DiscordWebhookUri = "DiscordWebhookUri";
    public static readonly string queueAddSubscription = "queueAddSubscription";
    public static readonly string queueRemoveSubscription = "queueRemoveSubscription";
    public static readonly string queueDiscordHandler = "queueDiscordHandler";
    public static readonly string queueEventOnFollow = "queueEventOnFollow";
    public static readonly string queueEventOnStreamOnline = "queueEventOnStreamOnline";
    public static readonly string queueEventOnStreamOffline = "queueEventOnStreamOffline";
    public static readonly string queueTwitterHandler = "queueTwitterHandler";
    public static readonly string Twitch_CallbackUrl = "Twitch_CallbackUrl";
    public static readonly string Twitch_ClientId = "Twitch_ClientId";
    public static readonly string Twitch_ClientSecret = "Twitch_ClientSecret";
    public static readonly string Twitch_SignatureSecret = "Twitch_SignatureSecret";
    public static readonly string TwitterConsumerKey = "TwitterConsumerKey";
    public static readonly string TwitterConsumerSecret = "TwitterConsumerSecret";
    public static readonly string TwitterAccessToken = "TwitterAccessToken";
    public static readonly string TwitterAccessTokenSecret = "TwitterAccessTokenSecret";
    public static readonly string TwitterUseV2API = "TwitterUseV2API";
    public static readonly string TwitterTemplateOnFollow = "TwitterTemplateOnFollow";
    public static readonly string TwitterTemplateOnStreamOnline = "TwitterTemplateOnStreamOnline";
    public static readonly string TwitterTemplateOnStreamOffline = "TwitterTemplateOnStreamOffline";
    public static readonly string tableTwichLiveNotificationsConfiguration = "TwichLiveNotificationsConfiguration";
    public static readonly string QueueServiceStorageAccount = "StorageQueueConnection__queueServiceUri";
    public static readonly string TableServiceStorageAccount = "StorageTableConnection__tableServiceUri";
    public static readonly string TwitterUri = "https://api.twitter.com/2/tweets";
}
