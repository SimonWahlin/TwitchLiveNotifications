using Azure.Data.Tables;
using Azure.Identity;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using Twitch.Net.Api;
using Twitch.Net.EventSub;
using TwitchLiveNotifications.Helpers;

namespace TwitchLiveNotifications
{
    public class Program
    {
        public static void Main()
        {
            var host = new HostBuilder()
                .ConfigureFunctionsWorkerDefaults()
                .ConfigureServices(s =>
                {
                    s.AddHttpClient();
                    s.AddAzureClients(builder =>
                    {
                        builder.AddQueueServiceClient(
                            new Uri(Environment.GetEnvironmentVariable(ConfigValues.QueueServiceStorageAccount))
                        ).ConfigureOptions(c => c.MessageEncoding = Azure.Storage.Queues.QueueMessageEncoding.Base64);
                        builder.UseCredential(new DefaultAzureCredential());
                    });

                    s.AddSingleton<TableClient>(new TableClient(
                        new Uri(Environment.GetEnvironmentVariable(ConfigValues.TableServiceStorageAccount)),
                        ConfigValues.tableTwichLiveNotificationsConfiguration,
                        new DefaultAzureCredential()));

                    s.AddTwitchEventSubService(config =>
                    {
                        config.ClientId = Environment.GetEnvironmentVariable(ConfigValues.Twitch_ClientId);
                        config.ClientSecret = Environment.GetEnvironmentVariable(ConfigValues.Twitch_ClientSecret);
                        config.CallbackUrl = Environment.GetEnvironmentVariable(ConfigValues.Twitch_CallbackUrl);
                        config.SignatureSecret = Environment.GetEnvironmentVariable(ConfigValues.Twitch_SignatureSecret);
                    });
                    s.AddTwitchApiClient(config =>
                    {
                        config.ClientId = Environment.GetEnvironmentVariable(ConfigValues.Twitch_ClientId);
                        config.ClientSecret = Environment.GetEnvironmentVariable(ConfigValues.Twitch_ClientSecret);
                    });
                    s.AddHostedService<TwitchNotificationService>();
                    s.AddTransient<EventSubBuilder>();
                })
                .Build();
            host.Run();
        }
    }
}