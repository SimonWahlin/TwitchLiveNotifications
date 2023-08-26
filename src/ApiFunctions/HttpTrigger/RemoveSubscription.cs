using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Twitch.Net.EventSub;

namespace TwitchLiveNotifications.EventSubFunctions;

public class RemoveSubscription
{
    private readonly ILogger _logger;
    private readonly IEventSubService2 _eventSubService;

    public RemoveSubscription(ILoggerFactory loggerFactory, IEventSubService2 eventSubService)
    {
        _logger = loggerFactory.CreateLogger<RemoveSubscription>();
        _eventSubService = eventSubService;
    }

    [Function("RemoveSubscription")]
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
