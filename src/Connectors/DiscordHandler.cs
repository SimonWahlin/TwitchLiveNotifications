using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Threading.Tasks;
using TwitchLiveNotifications.Helpers;
using TwitchLiveNotifications.Models;

namespace TwitchLiveNotifications.Connectors
{
    public class DiscordHandler
    {
        private readonly ILogger _logger;
        private readonly IHttpClientFactory _httpClientFactory;

        public DiscordHandler
            (ILoggerFactory loggerFactory, IHttpClientFactory httpClientFactory)
        {
            _logger = loggerFactory.CreateLogger<DiscordHandler>();
            _httpClientFactory = httpClientFactory; 
        }

        [Function("DiscordHandler")]
        public async Task<DiscordMessage> Run([QueueTrigger("%queueDiscordHandler%", Connection = "StorageQueueConnection")] DiscordMessage message)
        {
            var discordClient = new DiscordClient(_logger, _httpClientFactory);
            await discordClient.SendDiscordMessageAsync(message);
            return message;
        }
    }
}
