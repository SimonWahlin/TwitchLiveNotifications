using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Optional.Unsafe;
using System.Threading.Tasks;
using Twitch.Net.EventSub;
using TwitchLiveNotifications.Models;

namespace TwitchLiveNotifications.EventSubFunctions;

public class AddSubscription
{
    private readonly ILogger _logger;
    private readonly IEventSubService2 _eventSubService;
    private readonly EventSubBuilder _eventSubBuilder;

    public AddSubscription(ILoggerFactory loggerFactory, IEventSubService2 eventSubService, EventSubBuilder subBuilder)
    {
        _logger = loggerFactory.CreateLogger<AddSubscription>();
        _eventSubService = eventSubService;
        _eventSubBuilder = subBuilder;
    }

    [Function("AddSubscription")]
    public async Task<TwitchSubscription> Run([QueueTrigger("%queueAddSubscription%", Connection = "StorageQueueConnection")] TwitchSubscription subscription)
    {
        var model = _eventSubBuilder.Build(subscription.Type, subscription.Value);
        var result = await _eventSubService.Subscribe(model.ValueOrFailure());

        if (result.AlreadyRegistered)
        {
            _logger.LogInformation("{subscription} was already registered.", subscription.Value);
            return subscription;
        }

        _logger.LogInformation("{subscription} successfully registered.", subscription.Value);
        return subscription;
    }
}
