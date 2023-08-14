using Azure.Storage.Queues;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Twitch.Net.EventSub;
using Twitch.Net.EventSub.Notifications;
using TwitchLiveNotifications.EventSubFunctions;

namespace TwitchLiveNotifications.Helpers;

internal class TwitchNotificationService : IHostedService
{
    private readonly ILogger _logger;
    private readonly IEventSubService2 _eventSubService;
    private readonly QueueServiceClient _queueClientService;

    public TwitchNotificationService(ILoggerFactory loggerFactory, IEventSubService2 eventSubService, QueueServiceClient queueClientService)
    {
        _logger = loggerFactory.CreateLogger<SubscriptionCallBack>();
        _eventSubService = eventSubService;
        _queueClientService = queueClientService;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _eventSubService.Events.OnFollowed += EventOnFollowed;
        _eventSubService.Events.OnStreamOnline += EventOnStreamOnline;
        _eventSubService.Events.OnStreamOffline += EventOnStreamOffline;
        return Task.CompletedTask;
    }

    private Task EventOnFollowed(NotificationEvent<ChannelFollowNotificationEvent> arg)
    {
        string queueNameVariable = Constants.QueueEventOnFollow;
        _logger.LogInformation("[UserFollowedEvent] {UserName}#{UserId} followed {BroadcasterUserName}#{BroadcasterId}", arg.Event.UserName, arg.Event.UserId, arg.Event.BroadcasterUserName, arg.Event.BroadcasterId);

        var message = JsonSerializer.Serialize(arg.Event);
        SendMessage(queueNameVariable, message);
        return Task.CompletedTask;
    }

    private Task EventOnStreamOnline(NotificationEvent<StreamOnlineNotificationEvent> arg)
    {
        string queueNameVariable = Constants.QueueEventOnStreamOnline;
        _logger.LogInformation("[StreamOnlineEvent] {BroadcasterUserName}#{BroadcasterIdString} is streaming with stream id: {EventId}", arg.Event.BroadcasterUserName, arg.Event.BroadcasterIdString, arg.Event.Id);

        var message = JsonSerializer.Serialize(arg.Event);
        SendMessage(queueNameVariable, message);
        return Task.CompletedTask;
    }

    private Task EventOnStreamOffline(NotificationEvent<StreamOfflineNotificationEvent> arg)
    {
        string queueNameVariable = Constants.QueueEventOnStreamOffline;
        _logger.LogInformation("[StreamOfflineEvent] {BroadcasterUserName}#{BroadcasterIdString} stopped streaming.", arg.Event.BroadcasterUserName, arg.Event.BroadcasterIdString);

        var message = JsonSerializer.Serialize(arg.Event);
        SendMessage(queueNameVariable, message);
        return Task.CompletedTask;
    }

    private void SendMessage(string queueNameVariable, string message)
    {
        string queueName = Environment.GetEnvironmentVariable(queueNameVariable);
        if (string.IsNullOrEmpty(queueName))
        {
            _logger.LogInformation("Environment variable '{queueNameVariable}' is empty, no message posted", queueNameVariable);
            return;
        }

        var queueClient = _queueClientService.GetQueueClient(queueName);
        queueClient.CreateIfNotExists();
        queueClient.SendMessage(message);
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}