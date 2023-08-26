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

/// <summary>
/// This is a work in progress and sadly not working yet.
/// </summary>
[SuppressMessage("CodeQuality", "IDE0052:Remove unread private members", Justification = "<Pending>")]
[SuppressMessage("Roslynator", "RCS1155:Use StringComparison when comparing strings.", Justification = "<Pending>")]
[SuppressMessage("Major Code Smell", "S125:Sections of code should not be commented out", Justification = "<Pending>")]
[SuppressMessage("Critical Code Smell", "S4487:Unread \"private\" fields should be removed", Justification = "<Pending>")]
[SuppressMessage("Major Code Smell", "S1144:Unused private types or members should be removed", Justification = "<Pending>")]
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
