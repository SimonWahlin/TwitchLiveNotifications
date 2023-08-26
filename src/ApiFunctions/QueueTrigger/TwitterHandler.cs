using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using TwitchLiveNotifications.Helpers;
using TwitchLiveNotifications.Models;

namespace TwitchLiveNotifications.Connectors;

public class TwitterHandler
{
    private readonly ILogger _logger;

    public TwitterHandler(ILoggerFactory loggerFactory)
    {
        _logger = loggerFactory.CreateLogger<TwitterHandler>();
    }

    [Function("TwitterHandler")]
    public async Task Run([QueueTrigger("%queueTwitterHandler%", Connection = "StorageQueueConnection")] TweetMessage message)
    {
        TwitterClientV2 twitterClient = new(_logger);
        await twitterClient.PostTweet(message);
    }
}
