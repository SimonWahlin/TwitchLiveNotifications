using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Twitch.Net.EventSub;

namespace TwitchLiveNotifications.EventSubFunctions
{
    public class RemoveEventSub
    {
        private readonly ILogger _logger;
        private readonly IEventSubService2 _eventSubService;

        public RemoveEventSub(ILoggerFactory loggerFactory, IEventSubService2 eventSubService)
        {
            _logger = loggerFactory.CreateLogger<AddEventSub>();
            _eventSubService = eventSubService;
        }

        [Function("RemoveEventSub")]
        public async Task<string> Run([QueueTrigger("%queueRemoveSubscription%", Connection = "StorageQueueConnection")] string id)
        {
            var result = await _eventSubService.Unsubscribe(id);

            if (result)
            {
                _logger.LogInformation("{subscription} was unregistered.", id);
                return id;
            }

            return "";
        }
    }
}
