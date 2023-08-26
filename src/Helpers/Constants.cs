namespace TwitchLiveNotifications.Helpers;

internal static class Constants
{
    // List of config values and which environment variable they are stored in.
    public static readonly string DisableNotifications = "DISABLE_NOTIFICATIONS";
    public static readonly string DiscordTemplateOnFollow = "DiscordTemplateOnFollow";
    public static readonly string DiscordTemplateOnStreamOnline = "DiscordTemplateOnStreamOnline";
    public static readonly string DiscordTemplateOnStreamOffline = "DiscordTemplateOnStreamOffline";
    public static readonly string DiscordWebhookUri = "DiscordWebhookUri";
    public static readonly string QueueAddSubscription = "queueAddSubscription";
    public static readonly string QueueRemoveSubscription = "queueRemoveSubscription";
    public static readonly string QueueDiscordHandler = "queueDiscordHandler";
    public static readonly string QueueEventOnFollow = "queueEventOnFollow";
    public static readonly string QueueEventOnStreamOnline = "queueEventOnStreamOnline";
    public static readonly string QueueEventOnStreamOffline = "queueEventOnStreamOffline";
    public static readonly string QueueTwitterHandler = "queueTwitterHandler";
    public static readonly string TwitchCallbackUrl = "Twitch_CallbackUrl";
    public static readonly string TwitchClientId = "Twitch_ClientId";
    public static readonly string TwitchClientSecret = "Twitch_ClientSecret";
    public static readonly string TwitchSignatureSecret = "Twitch_SignatureSecret";
    public static readonly string TwitterConsumerKey = "TwitterConsumerKey";
    public static readonly string TwitterConsumerSecret = "TwitterConsumerSecret";
    public static readonly string TwitterAccessToken = "TwitterAccessToken";
    public static readonly string TwitterAccessTokenSecret = "TwitterAccessTokenSecret";
    public static readonly string TwitterUseV2API = "TwitterUseV2API";
    public static readonly string TwitterTemplateOnFollow = "TwitterTemplateOnFollow";
    public static readonly string TwitterTemplateOnStreamOnline = "TwitterTemplateOnStreamOnline";
    public static readonly string TwitterTemplateOnStreamOffline = "TwitterTemplateOnStreamOffline";
    public static readonly string TableTwichLiveNotificationsConfiguration = "TwichLiveNotificationsConfiguration";
    public static readonly string QueueServiceStorageAccount = "StorageQueueConnection__queueServiceUri";
    public static readonly string TableServiceStorageAccount = "StorageTableConnection__tableServiceUri";
#pragma warning disable S1075 // URIs should not be hardcoded
    public static readonly string TwitterUri = "https://api.twitter.com/2/tweets";
#pragma warning restore S1075 // URIs should not be hardcoded
}
