using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using Tweetinvi.Exceptions;
using Tweetinvi.Models;
using TwitchLiveNotifications.Models;

namespace TwitchLiveNotifications.Helpers
{
    public static class TwitterClientV1
    {
        private static readonly bool Disable_Notifications = Environment.GetEnvironmentVariable(ConfigValues.DISABLE_NOTIFICATIONS).ToLower() == "true";
        private readonly static string ConsumerKey = Environment.GetEnvironmentVariable(ConfigValues.TwitterConsumerKey);
        private readonly static string ConsumerSecret = Environment.GetEnvironmentVariable(ConfigValues.TwitterConsumerSecret);
        private readonly static string AccessToken = Environment.GetEnvironmentVariable(ConfigValues.TwitterAccessToken);
        private readonly static string AccessTokenSecret = Environment.GetEnvironmentVariable(ConfigValues.TwitterAccessTokenSecret);
        public const int MaxTweetLength = 280;

        public static async Task<ITweet> PublishTweet(TweetMessage TweetMessage, ILogger log)
        {
            log.LogInformation("PublishTweet Tweet: {TweetMessage}", TweetMessage);

            if (TweetMessage.Text.Length > MaxTweetLength)
            {
                log.LogWarning("PublishTweet Tweet too long {Length} max {MaxTweetLength}", TweetMessage.Text.Length, MaxTweetLength);
            }

            if (Disable_Notifications)
            {
                log.LogInformation("PublishTweet Notifications are disabled. exiting");
                return null;
            }

            try
            {
                var tweetinvi = new Tweetinvi.TwitterClient(ConsumerKey, ConsumerSecret, AccessToken, AccessTokenSecret);
                var publishedTweet = await tweetinvi.Tweets.PublishTweetAsync(TweetMessage.Text);
                log.LogInformation("PublishTweet published tweet {id}",publishedTweet.Id);
                return publishedTweet;
            }
            catch (TwitterException e)
            {
                log.LogError("Failed to tweet: {error}", e.ToString());
            }
            catch (Exception e)
            {
                log.LogError("Unhandled error when sending tweet: {error}", e.ToString());
                throw;
            }
            return null;
        }
    }
}
