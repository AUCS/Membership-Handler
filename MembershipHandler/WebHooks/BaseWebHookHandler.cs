using Microsoft.AspNet.WebHooks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using Microsoft.Azure;

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
        
    }
}