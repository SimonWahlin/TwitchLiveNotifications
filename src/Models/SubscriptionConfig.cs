using Azure;
using Azure.Data.Tables;
using System;
using System.Linq;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace TwitchLiveNotifications.Models;

public class SubscriptionConfig : ITableEntity
{

    [JsonPropertyName("twitchname")]
    public string TwitchName { get; set; }

    [JsonPropertyName("twittername")]
    public string TwitterName { get; set; }

    [JsonPropertyName("discordname")]
    public string DiscordName { get; set; }

    public string JoinedTitleFilters
    {
        get { return string.Join(',', TitleFilters); }
        set
        {
            if (value != null)
            {
                TitleFilters = value.Split(',');
            }
        }
    }

    public string JoinedCategoryFilters
    {
        get { return string.Join(',', CategoryFilters); }
        set
        {
            if (value != null)
            {
                CategoryFilters = value.Split(',');
            }
        }
    }

    [IgnoreDataMember]
    [JsonPropertyName("titlefilters")]
    public string[] TitleFilters { get; set; } = new string[0];

    [IgnoreDataMember]
    [JsonPropertyName("categoryfilters")]
    public string[] CategoryFilters { get; set; } = new string[0];

    public string TwitchId { get; set; }

    public string PartitionKey { get; set; }
    public string RowKey { get; set; }
    public DateTimeOffset? Timestamp { get; set; }
    public ETag ETag { get; set; }
    public static SubscriptionConfig GetTwitchSubscriptionConfiguration(string channelId, TableClient tableClient)
    {
        tableClient.CreateIfNotExists();
        var config = tableClient.Query<SubscriptionConfig>(e => e.PartitionKey == "TwitchSubscriptionConfig" && e.RowKey == channelId.ToLower()).FirstOrDefault();
        return config;
    }

    public static void SetTwitchSubscriptionConfiguration(SubscriptionConfig config, TableClient tableClient)
    {
        tableClient.CreateIfNotExists();
        config.PartitionKey = "TwitchSubscriptionConfig";
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
