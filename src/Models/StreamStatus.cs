using Azure;
using Azure.Data.Tables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace TwitchLiveNotifications.Models;

public class StreamStatusEntry : ITableEntity
{
    private string partitionKey;
    private string rowKey;
    private DateTimeOffset? timeStamp;
    private ETag etag;

    [JsonPropertyName("broadcasterId")]
    public string BroadCasterId { get; set; }

    [JsonPropertyName("broadcasterName")]
    public string BoradCasterName { get; set; }

    [JsonPropertyName("streamUri")]
    public string StreamUri { get; set; }

    [JsonPropertyName("title")]
    public string Title { get; set; }

    [JsonPropertyName("game")]
    public string Game { get; set; }

    [JsonPropertyName("startedat")]
    public DateTime StartedAt { get; set; }

    [JsonPropertyName("status")]
    public StreamStatus Status { get; set; }

    public string PartitionKey { get => partitionKey; set => partitionKey = value; }
    public string RowKey { get => rowKey; set => rowKey = value; }
    public DateTimeOffset? Timestamp { get => timeStamp; set => timeStamp = value; }
    public ETag ETag { get => etag; set => etag = value; }

    public static List<StreamStatusEntry> GetTwitchStreamStatus(TableClient tableClient)
    {
        var result = new List<StreamStatusEntry>();
        tableClient.CreateIfNotExists();
        var config = tableClient.Query<StreamStatusEntry>(e => e.PartitionKey == "TwitchStreamStatusOnline");
        foreach(var entry in config)
        {
            result.Add(entry);
        }
        return result;
    }

    public static StreamStatusEntry GetTwitchStreamStatus(string channelId, TableClient tableClient)
    {
        tableClient.CreateIfNotExists();
        var config = tableClient.Query<StreamStatusEntry>(e => e.PartitionKey == "TwitchStreamStatusOnline" && e.RowKey == channelId.ToLower()).FirstOrDefault();
        return config;
    }

    public static async Task SetTwitchStreamStatusAsync(StreamStatusEntry entry, TableClient tableClient)
    {
        tableClient.CreateIfNotExists();
        entry.PartitionKey = "TwitchStreamStatusOnline";
        entry.RowKey = entry.BroadCasterId.ToLower();

        if (entry.Status == StreamStatus.Online)
        {
            _ = await tableClient.UpsertEntityAsync(entry, TableUpdateMode.Replace);
        }
        else
        {
            _ = await tableClient.DeleteEntityAsync(entry.partitionKey, entry.RowKey);
        }
    }
}

public enum StreamStatus
{
    Online,
    Offline
}
