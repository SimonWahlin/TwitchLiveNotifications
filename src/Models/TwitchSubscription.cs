namespace TwitchLiveNotifications.Models
{
    public class TwitchSubscription
    {
        public string Type { get; set; }
        public string Value { get; set; }

        public override string ToString() => $"{Type}:{Value}";
    }
}
