using System.Reflection.Metadata;
using Azure.Data.Tables;
using Azure.Identity;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using Twitch.Net.Api;
using Twitch.Net.EventSub;
using TwitchLiveNotifications.Helpers;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureServices(s =>
    {
        s.AddHttpClient();
#if DEBUG // When running locally, we are using well-known connections strings instead of Azure Identities to make debugging easier
        s.AddAzureClients(builder =>
        {
            builder.AddQueueServiceClient(
                "AccountName=devstoreaccount1;AccountKey=Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==;DefaultEndpointsProtocol=http;BlobEndpoint=http://127.0.0.1:10000/devstoreaccount1;QueueEndpoint=http://127.0.0.1:10001/devstoreaccount1;TableEndpoint=http://127.0.0.1:10002/devstoreaccount1;"
            ).ConfigureOptions(c => c.MessageEncoding = Azure.Storage.Queues.QueueMessageEncoding.Base64);
        });

        s.AddSingleton(new TableClient(
            "AccountName=devstoreaccount1;AccountKey=Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==;DefaultEndpointsProtocol=http;BlobEndpoint=http://127.0.0.1:10000/devstoreaccount1;QueueEndpoint=http://127.0.0.1:10001/devstoreaccount1;TableEndpoint=http://127.0.0.1:10002/devstoreaccount1;",
            Constants.TableTwichLiveNotificationsConfiguration
        ));
#else
        s.AddAzureClients(builder =>
        {
            builder.AddQueueServiceClient(
                new Uri(Environment.GetEnvironmentVariable(Constants.QueueServiceStorageAccount))
            ).ConfigureOptions(c => c.MessageEncoding = Azure.Storage.Queues.QueueMessageEncoding.Base64);
            builder.UseCredential(new DefaultAzureCredential());
        });

        s.AddSingleton<TableClient>(new TableClient(
            new Uri(Environment.GetEnvironmentVariable(Constants.TableServiceStorageAccount)),
            Constants.TableTwichLiveNotificationsConfiguration,
            new DefaultAzureCredential()));
#endif

        s.AddTwitchEventSubService(config =>
        {
            config.ClientId = Environment.GetEnvironmentVariable(Constants.TwitchClientId);
            config.ClientSecret = Environment.GetEnvironmentVariable(Constants.TwitchClientSecret);
            config.CallbackUrl = Environment.GetEnvironmentVariable(Constants.TwitchCallbackUrl);
            config.SignatureSecret = Environment.GetEnvironmentVariable(Constants.TwitchSignatureSecret);
        });
        s.AddTwitchApiClient(config =>
        {
            config.ClientId = Environment.GetEnvironmentVariable(Constants.TwitchClientId);
            config.ClientSecret = Environment.GetEnvironmentVariable(Constants.TwitchClientSecret);
        });
        s.AddHostedService<TwitchNotificationService>();
        s.AddTransient<EventSubBuilder>();
    })
    .Build();
host.Run();