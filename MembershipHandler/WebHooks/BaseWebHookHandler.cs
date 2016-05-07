using Microsoft.AspNet.WebHooks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using Microsoft.Azure;
using MembershipHandler.Models;

namespace MembershipHandler.WebHooks
{
    public abstract class BaseWebHookHandler : WebHookHandler
    {
        private CloudStorageAccount storageAccount;
        protected CloudTableClient tableClient;
        
        public BaseWebHookHandler()
        {
            // Retrieve the storage account from the connection string.
            storageAccount = CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting("StorageConnectionString"));
            // Create the table client.
            tableClient = storageAccount.CreateCloudTableClient();
        }

        protected string GetCommitteeTitle(string aucsId)
        {
            CloudTable table = tableClient.GetTableReference("Committee");
            table.CreateIfNotExists();
            TableQuery<DynamicTableEntity> results = new TableQuery<DynamicTableEntity>()
                .Where(TableQuery.GenerateFilterCondition("AUCSID", QueryComparisons.Equal, aucsId))
                .Select(new string[] { "Title" });
            return table.ExecuteQuery(results).Select(q => q.Properties["Title"].StringValue).FirstOrDefault();
        }
    }
}