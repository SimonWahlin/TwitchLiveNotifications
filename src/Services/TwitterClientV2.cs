using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Tweetinvi;
using Tweetinvi.Core.Web;
using Tweetinvi.Models;
using TwitchLiveNotifications.Models;

namespace TwitchLiveNotifications.Helpers;

public class TwitterClientV2
{
    private readonly ITwitterClient _client;
    private static readonly bool Disable_Notifications = Environment.GetEnvironmentVariable(Constants.DisableNotifications).ToLower() == "true";
    private readonly static string ConsumerKey = Environment.GetEnvironmentVariable(Constants.TwitterConsumerKey);
    private readonly static string ConsumerSecret = Environment.GetEnvironmentVariable(Constants.TwitterConsumerSecret);
    private readonly static string AccessToken = Environment.GetEnvironmentVariable(Constants.TwitterAccessToken);
    private readonly static string AccessTokenSecret = Environment.GetEnvironmentVariable(Constants.TwitterAccessTokenSecret);
    private const int MaxMessageSize = 280;
    private readonly ILogger _logger;

    public TwitterClientV2(ILogger logger)
    {
        _logger = logger;
        TwitterCredentials userCredentials = new(ConsumerKey, ConsumerSecret, AccessToken, AccessTokenSecret);
        _client = new TwitterClient(credentials: userCredentials);
    }

    public Task<ITwitterResult> PostTweet(TweetMessage tweetParams)
    {
        return this._client.Execute.AdvanceRequestAsync(
            (ITwitterRequest request) =>
            {
                var jsonBody = JsonSerializer.Serialize(tweetParams);
                var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");
                request.Query.Url = "https://api.twitter.com/2/tweets";
                request.Query.HttpMethod = Tweetinvi.Models.HttpMethod.POST;
                request.Query.HttpContent = content;
            }
        );
    }
}
