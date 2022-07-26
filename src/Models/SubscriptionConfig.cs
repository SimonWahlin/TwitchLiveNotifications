using Azure;
using Azure.Data.Tables;
using System;
using System.Linq;
using System.Runtime.Serialization;
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

    public string JoinedTitleFilters
    {
        get { return string.Join(',', TitleFilters); }
        set { TitleFilters = value.Split(','); }
    }

    public string JoinedCategoryFilters
    {
        get { return string.Join(',', CategoryFilters); }
        set { CategoryFilters = value.Split(','); }
    }

    [IgnoreDataMember]
    [JsonPropertyName("titlefilters")]
    public string[] TitleFilters { get; set; }

    [IgnoreDataMember]
    [JsonPropertyName("categoryfilters")]
    public string[] CategoryFilters { get; set; }

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
        if (TitleFilters != null && TitleFilters.Count() > 0)
        {
            return TitleFilters.Any(s => title.ToLower().Contains(s.ToLower()));
        }
        if (CategoryFilters != null && CategoryFilters.Count() > 0)
        {
            return CategoryFilters.Any(s => category.ToLower().Contains(s.ToLower()));
        }

        return true;
    }

}
