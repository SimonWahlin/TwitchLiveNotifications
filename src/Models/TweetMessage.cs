using System.Text.Json.Serialization;

namespace TwitchLiveNotifications.Models
{
    public class TweetMessage
    {
        [JsonPropertyName("text")]
        public string Text { get; set; }
    }
}
