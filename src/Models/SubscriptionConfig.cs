using Azure;
using Azure.Data.Tables;
using System;
using System.Linq;
using System.Text.Json.Serialization;

namespace TwitchLiveNotifications.Models;

public class SubscriptionConfig : ITableEntity
{
    private string partitionKey;
    private string rowKey;
    private DateTimeOffset? timeStamp;
    private ETag etag;

    [JsonPropertyName("twitchname")]
    public string TwitchName { get; set; }

    [JsonPropertyName("twittername")]
    public string TwitterName { get; set; }

    [JsonPropertyName("discordname")]
    public string DiscordName { get; set; }

    [JsonPropertyName("keywordfilter")]
    public string[] KeywordFilter { get; set; }

    [JsonPropertyName("categoryfilter")]
    public string[] CategoryFilter { get; set; }

    public string TwitchId { get; set; }

    public string PartitionKey { get => partitionKey; set => partitionKey = value; }
    public string RowKey { get => rowKey; set => rowKey = value; }
    public DateTimeOffset? Timestamp { get => timeStamp; set => timeStamp = value; }
    public ETag ETag { get => etag; set => etag = value; }
    public static SubscriptionConfig GetTwitchSubscriptionConfiguration(string channelId, TableClient tableClient)
    {
        tableClient.CreateIfNotExists();
        var config = tableClient.Query<SubscriptionConfig>(e => e.PartitionKey == "TwitchSubscriptionConfig" && e.RowKey == channelId.ToLower()).FirstOrDefault();
        return config;
    }

    public static void SetTwitchSubscriptionConfiguration(SubscriptionConfig config, TableClient tableClient)
    {
        tableClient.CreateIfNotExists();
        config.partitionKey = "TwitchSubscriptionConfig";
        config.RowKey = config.TwitchId.ToLower();
        tableClient.UpsertEntity(config, TableUpdateMode.Replace);
    }
    public bool ShouldSendNotification(string title, string category)
    {
        if (KeywordFilter != null && KeywordFilter.Count() > 0)
        {
            return KeywordFilter.Any(s => title.ToLower().Contains(s.ToLower()));
        }
        if (CategoryFilter != null && CategoryFilter.Count() > 0)
        {
            return CategoryFilter.Any(s => category.ToLower().Contains(s.ToLower()));
        }

        return true;
    }

}
