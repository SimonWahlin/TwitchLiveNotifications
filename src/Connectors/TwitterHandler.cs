using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Threading.Tasks;
using TwitchLiveNotifications.Helpers;
using TwitchLiveNotifications.Models;

namespace TwitchLiveNotifications
{
    public class TwitterHandler
    {
        private readonly ILogger _logger;

        public TwitterHandler(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<TwitterHandler>();
        }

        [Function("TwitterHandler")]
        public async Task<string> Run([QueueTrigger("%queueTwitterHandler%", Connection = "")] string message)
        {
            await TwitterClient.PublishTweet(message, _logger);
            return message;
        }
    }
}
