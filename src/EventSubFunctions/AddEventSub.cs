using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Optional.Unsafe;
using System.Threading.Tasks;
using Twitch.Net.EventSub;
using TwitchLiveNotifications.Models;
using TwitchLiveNotifications.Helpers;

namespace TwitchLiveNotifications.EventSubFunctions
{
    public class AddEventSub
    {
        private readonly ILogger _logger;
        private readonly IEventSubService2 _eventSubService;
        private readonly EventSubBuilder _evenetSubBuilder;

        public AddEventSub(ILoggerFactory loggerFactory, IEventSubService2 eventSubService, EventSubBuilder subBuilder)
        {
            _logger = loggerFactory.CreateLogger<AddEventSub>();
            _eventSubService = eventSubService;
            _evenetSubBuilder = subBuilder;
        }

        [Function("AddEventSub")]
        public async Task<TwitchSubscription> Run([QueueTrigger("%queueAddSubscription%", Connection = "")] TwitchSubscription subscription)
        {
            var model = _evenetSubBuilder.Build(subscription.Type, subscription.Value);
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
}
