using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using TwitchLiveNotifications.Models;

namespace TwitchLiveNotifications.Helpers
{
    public class DiscordClient
    {
        private static readonly string DiscordWebhookUri = Environment.GetEnvironmentVariable(ConfigValues.DiscordWebhookUri);
        private static readonly bool Disable_Notifications = Environment.GetEnvironmentVariable(ConfigValues.DISABLE_NOTIFICATIONS).ToLower() == "true";
        private const int MaxMessageSize = 2000;
        private readonly ILogger _logger;
        private readonly IHttpClientFactory _httpClientFactory;

        public DiscordClient(ILogger logger, IHttpClientFactory httpClientFactory)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;
        }

        public async Task<HttpResponseMessage> SendDiscordMessageAsync(DiscordMessage discordMessage)
        {
            _logger.LogInformation("SendDiscordMessageAsync DiscordMessage: {discordMessage}", discordMessage.Content);

            if (discordMessage.Content.Length >= MaxMessageSize)
            {
                _logger.LogError("SendDiscordMessageAsync Discord messages is {Length} long and exceeds the {MaxMessageSize} max length.", discordMessage.Content.Length, MaxMessageSize);
                return null;
            }

            if (Disable_Notifications)
            {
                _logger.LogInformation("SendDiscordMessageAsync Notifications are disabled. exiting");
                return null;
            }

            var client = _httpClientFactory.CreateClient();
            var httpMessageBody = JsonSerializer.Serialize(discordMessage);
            _logger.LogInformation("SendDiscordMessageAsync HttpMessageBody: {httpMessageBody}", httpMessageBody);

            var httpMessage = new HttpRequestMessage()
            {
                RequestUri = new Uri(DiscordWebhookUri),
                Content = new StringContent(httpMessageBody, Encoding.UTF8, "application/json"),
                Method = HttpMethod.Post
            };

            var httpResponse = await client.SendAsync(httpMessage, HttpCompletionOption.ResponseHeadersRead);

            if (!httpResponse.IsSuccessStatusCode)
            {
                _logger.LogError("SendDiscordMessageAsync Request Failed");
            }
            _logger.LogInformation("SendDiscordMessageAsync Success: {Success}", httpResponse.IsSuccessStatusCode);
            _logger.LogInformation("SendDiscordMessageAsync StatusCode: {StatusCode}", httpResponse.StatusCode);
            _logger.LogInformation("SendDiscordMessageAsync ReasonPhrase: {ReasonPhrase}", httpResponse.ReasonPhrase);

            var responseBody = await httpResponse.Content.ReadAsStringAsync();
            _logger.LogInformation("SendDiscordMessageAsync Response: {ResponseBody}", responseBody);

            return httpResponse;
        }
    }
}
