using TwitchLiveNotifications.Models;

namespace TwitchLiveNotifications.Tests
{
    public class ChannelConfigTests
    {
        [Theory]
        [InlineData("Something Blazor", "Science & technologies", new[] { "blazor", "Kliptok" }, new[] { "Playing game" }, true)]
        [InlineData("Working on Kliptok", "Science & technologies", new[] { "blazor", "Kliptok" }, new[] { "Science & technologies" }, true)]
        [InlineData("Just chatting", "Science & technologies", new[] { "blazor" }, new[] { "Science & technologies" }, false)]
        [InlineData("Just chatting", "Science & technologies", new string[0], new[] { "Science & technologies" }, true)]
        [InlineData("Just chatting", "Science & technologies", new string[0], new[] { "No match" }, false)]
        [InlineData("Just chatting", "Science & technologies", new string[0], new string[0], true)]
        public void ShouldSendNotificationTests(string title, string category, string[] keywords, string[] categories, bool expected)
        {
            SubscriptionConfig config = new() { CategoryFilters = categories, TitleFilters = keywords };
            Assert.Equal(expected, config.ShouldSendNotification(title, category));
        }
    }
}