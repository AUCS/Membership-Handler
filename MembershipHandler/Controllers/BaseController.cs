using Microsoft.Azure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Controllers;
using MembershipHandler.Models;

namespace MembershipHandler.Controllers
{
    public class BaseController : ApiController
    {
        private CloudStorageAccount storageAccount;
        protected CloudTableClient tableClient;

        public override Task<HttpResponseMessage> ExecuteAsync(HttpControllerContext controllerContext, CancellationToken cancellationToken)
        {
            // Retrieve the storage account from the connection string.
            storageAccount = CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting("StorageConnectionString"));
            // Create the table client.
            tableClient = storageAccount.CreateCloudTableClient();

            return base.ExecuteAsync(controllerContext, cancellationToken);
        }
    }
}
