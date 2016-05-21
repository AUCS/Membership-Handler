using MembershipHandler.Filters;
using MembershipHandler.Models;
using Microsoft.Azure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace MembershipHandler.Controllers
{
    public class ConfirmController : ApiController
    {
        [HttpGet]
        public HttpResponseMessage Get(string id)
        {
            if (id.Length != Guid.NewGuid().ToString().Length)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, "Invalid code.");
            }

            // Retrieve the storage account from the connection string.
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting("StorageConnectionString"));
            // Create the table client.
            CloudTableClient TableClient = storageAccount.CreateCloudTableClient();

            CloudTable confirmTable = TableClient.GetTableReference("Confirms");
            confirmTable.CreateIfNotExists();

            TableQuery<Confirm> matchingAccount = new TableQuery<Confirm>().Where(
                    TableQuery.GenerateFilterCondition("Code", QueryComparisons.Equal, id));
            Confirm confirm = confirmTable.ExecuteQuery(matchingAccount).FirstOrDefault();
            if (confirm == null)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, "Invalid code.");
            }

            CloudTable memberTable = TableClient.GetTableReference("Members");
            memberTable.CreateIfNotExists();
            TableOperation tableOperation = TableOperation.Retrieve<Member>("Member", confirm.RowKey);
            Member member = (Member)memberTable.Execute(tableOperation).Result;
            
            string result = "Error: This one's a doozy to do with code confirmation.";

            if (confirm.PartitionKey == "Email")
            {
                member.EmailAddress = confirm.Value;
                tableOperation = TableOperation.Replace(member);
                memberTable.Execute(tableOperation);
                result = "Thanks " + member.Name + ", " + member.EmailAddress + " has been confirmed as your email address.";
            }
            else if (confirm.PartitionKey == "StudentId")
            {
                member.StudentId = confirm.Value;
                tableOperation = TableOperation.Replace(member);
                memberTable.Execute(tableOperation);
                result = "Thanks " + member.Name + ", " + member.StudentId + " has been confirmed as your student id number.";
            }

            tableOperation = TableOperation.Delete(confirm);
            confirmTable.Execute(tableOperation);

            return Request.CreateResponse(HttpStatusCode.OK, result);
        }
    }
}
