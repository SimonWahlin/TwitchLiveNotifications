using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Tweetinvi;
using Tweetinvi.Core.Web;
using Tweetinvi.Models;
using TwitchLiveNotifications.Models;

namespace TwitchLiveNotifications.Helpers
{
    /// <summary>
    /// This is a work in progress and sadly not working yet.
    /// </summary>
    public class TwitterClientV2
    {
        private readonly ITwitterClient _client;
        private static readonly string TwitterUri = ConfigValues.TwitterUri;
        private static readonly bool Disable_Notifications = Environment.GetEnvironmentVariable(ConfigValues.DISABLE_NOTIFICATIONS).ToLower() == "true";
        private readonly static string ConsumerKey = Environment.GetEnvironmentVariable(ConfigValues.TwitterConsumerKey);
        private readonly static string ConsumerSecret = Environment.GetEnvironmentVariable(ConfigValues.TwitterConsumerSecret);
        private readonly static string AccessToken = Environment.GetEnvironmentVariable(ConfigValues.TwitterAccessToken);
        private readonly static string AccessTokenSecret = Environment.GetEnvironmentVariable(ConfigValues.TwitterAccessTokenSecret);
        private const int MaxMessageSize = 280;
        private readonly ILogger _logger;
        private readonly IHttpClientFactory _httpClientFactory;


        public TwitterClientV2(ILogger logger, IHttpClientFactory httpClientFactory)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;
            _client = new Tweetinvi.TwitterClient(ConsumerKey, ConsumerSecret, AccessToken, AccessTokenSecret);
        }

        public Task<ITwitterResult> PostTweet(TweetMessage tweetParams)
        {
            return this._client.Execute.AdvanceRequestAsync(
                (ITwitterRequest request) =>
                {
                    var jsonBody = JsonSerializer.Serialize(tweetParams);
                    //var jsonBody = this._client.Json.Serialize(tweetParams);

                    // Technically this implements IDisposable,
                    // but if we wrap this in a using statement,
                    // we get ObjectDisposedExceptions,
                    // even if we create this in the scope of PostTweet.
                    //
                    // However, it *looks* like this is fine.  It looks
                    // like Microsoft's HTTP stuff will call
                    // dispose on requests for us (responses may be another story).
                    // See also: https://stackoverflow.com/questions/69029065/does-stringcontent-get-disposed-with-httpresponsemessage
                    var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

                    request.Query.Url = "https://api.twitter.com/2/tweets";
                    request.Query.HttpMethod = Tweetinvi.Models.HttpMethod.POST;
                    request.Query.HttpContent = content;
                }
            );
        }
    }
}
