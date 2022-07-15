using Azure.Data.Tables;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Twitch.Net.EventSub;
using TwitchLiveNotifications.Helpers;

namespace TwitchLiveNotifications.EventSubFunctions
{
    public class SubscriptionCallBack
    {
        private readonly ILogger _logger;
        private readonly IEventSubService2 _eventSubService;
        private readonly TableClient _configTable;

        public SubscriptionCallBack(ILoggerFactory loggerFactory, IEventSubService2 eventSubService, TableClient configTable)
        {
            _logger = loggerFactory.CreateLogger<SubscriptionCallBack>();
            _eventSubService = eventSubService;
            _configTable = configTable;
        }

        [Function("SubscriptionCallBack")]
        public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequestData req,
           FunctionContext executionContext)
        {
            var requestBody = new StreamReader(req.Body).ReadToEndAsync();
            var type = req.Headers.GetValues(EventSubHeaderConst.MessageType).FirstOrDefault();
            var messageId = req.Headers.GetValues(EventSubHeaderConst.MessageId).FirstOrDefault();
            HttpStatusCode httpStatusCode = HttpStatusCode.OK;

            bool isDuplicate = false;
            string[] requestList = Array.Empty<string>();

            // For notification events, we need to keep track of duplicate requests.
            if (type == "notification")
            {
                requestList = RequestDuplicationHelper.GetRequestList(_configTable);
                isDuplicate = requestList.Where(id => id == messageId).Any();
            }

            if (!isDuplicate)
            {
                _logger.LogInformation($"No duplicate, processing event");
                var result = _eventSubService.Handle(req.Headers, await requestBody);
                httpStatusCode = result.StatusCode;

                RequestDuplicationHelper.UpdateRequestList(requestList, messageId, _configTable);
            }
            else
            {
                _logger.LogInformation($"Duplicate event, ignoring!");
            }

            var response = req.CreateResponse(httpStatusCode);
            response.Headers.Add("content-type", "text/plain");
            return response;
        }
    }
}
