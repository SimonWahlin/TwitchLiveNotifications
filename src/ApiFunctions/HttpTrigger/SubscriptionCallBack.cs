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

namespace TwitchLiveNotifications.EventSubFunctions;

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
    public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequestData req,
       FunctionContext executionContext)
    {
        var type = req.Headers.GetValues(EventSubHeaderConst.MessageType).FirstOrDefault();
        var messageId = req.Headers.GetValues(EventSubHeaderConst.MessageId).FirstOrDefault();
        string[] requestList = Array.Empty<string>();

#if DEBUG
#else
        // For notification events, we need to keep track of duplicate requests.
        if (type == "notification")
        {
            requestList = RequestDuplicationHelper.GetRequestList(_configTable);
            if (requestList.Where(id => id == messageId).Any())
            {
                _logger.LogInformation("Duplicate event, ignoring!");
                return req.CreateResponse(HttpStatusCode.OK);
            }
        }
#endif
        var requestBody = await (new StreamReader(req.Body).ReadToEndAsync());
        _logger.LogInformation("Got callback with {messageId} of type {type} and body: {requestBody}", messageId, type, requestBody);

        var result = _eventSubService.Handle(req.Headers, requestBody);
        string responseBody = result?.CallBack?.Challenge ?? "";
        RequestDuplicationHelper.UpdateRequestList(requestList, messageId, _configTable);

        var response = req.CreateResponse(result.StatusCode);
        response.Headers.Add("content-type", "text/plain");
        response.WriteString(responseBody);
        return response;
    }
}
