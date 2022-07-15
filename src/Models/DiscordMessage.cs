using System.Text.Json.Serialization;

namespace TwitchLiveNotifications.Models
{
    public class DiscordMessage
    {
        [JsonPropertyName("content")]
        public string Content { get; set; }
    }
}
