using Azure.Data.Tables;
using System;
using System.Collections.Generic;
using System.Linq;

namespace TwitchLiveNotifications.Helpers;

internal static class RequestDuplicationHelper
{
    internal static string[] GetRequestList(TableClient tableClient)
    {
        tableClient.CreateIfNotExists();

        return tableClient.Query<TableEntity>(e => e.PartitionKey == "SubscriptionCallBack_requestHistory" && e.RowKey == "list")
            .Select(e =>
            {
                return e["value"].ToString();
            })
            .FirstOrDefault("")?.Split(',');
    }

    internal static void UpdateRequestList(string[] requestList, string messageId, TableClient tableClient)
    {
        // No need to keep more than 100 requests
        int skipCount = Math.Max(requestList.Length - 99, 0);
        string newRequestList = string.Join(',', requestList.Skip(skipCount).Append(messageId));

        var tableEntity = new TableEntity
        {
            { "PartitionKey", "SubscriptionCallBack_requestHistory" },
            { "RowKey", "list" },
            { "value", newRequestList }
        };
        var transactions = new List<TableTransactionAction>
        {
            new TableTransactionAction(TableTransactionActionType.UpsertReplace, tableEntity)
        };
        tableClient.SubmitTransaction(transactions);
    }
}
