using MembershipHandler.Models;
using MembershipHandler.Views;
using Microsoft.Azure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace MembershipHandler.Controllers
{
    public class RefreshWebsiteCacheController : BaseFacebookController
    {
        [HttpPost]
        public string Post()
        {
            if (!CurrentUser.Committee)
            {
                return "unauthorised";
            }
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting("StorageConnectionString"));
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
            CloudBlobContainer cacheContainer = blobClient.GetContainerReference("WebsiteCache");
            cacheContainer.CreateIfNotExists();

            RefreshCommitteeList(cacheContainer);

            return "success";
        }

        [NonAction]
        private void RefreshCommitteeList(CloudBlobContainer container)
        {
            CloudTable committeeTable = TableClient.GetTableReference("Committee");
            committeeTable.CreateIfNotExists();

            TableQuery<Committee> allCommitteeQuery = new TableQuery<Committee>().Where(
                    TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, "Committee"));
            List<Committee> allCommittee = committeeTable.ExecuteQuery(allCommitteeQuery).ToList();

            CommitteeList committeeList = allCommittee.ToCommitteeList(this);
                        
            CloudBlockBlob blockBlob = container.GetBlockBlobReference("CommitteeList");

            blockBlob.UploadText(committeeList.Serialise());
        }
    }
}
