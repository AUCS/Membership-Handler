using MembershipHandler.Models;
using Microsoft.Azure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace MembershipHandler.Controllers
{
    public class EmailConfirmController : ApiController
    {
        [HttpGet]
        public HttpResponseMessage Get(string guid)
        {
            // Retrieve the storage account from the connection string.
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(
                CloudConfigurationManager.GetSetting("StorageConnectionString"));
            // Create the table client.
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();
            // Retrieve a reference to the table.
            CloudTable table = tableClient.GetTableReference("Members");
            // Create the table if it doesn't exist.
            table.CreateIfNotExists();
            // Create the table query.
            TableQuery<Member> emailExistsQuery = new TableQuery<Member>().Where(
                    TableQuery.GenerateFilterCondition("ConfirmEmailId", QueryComparisons.Equal, guid));
            List<Member> results = table.ExecuteQuery(emailExistsQuery).ToList();
            if (results.Count < 1)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, " ");
            }
            if (results[0].EmailConfirmed)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, " ");
            }

            Member member = results[0];
            member.EmailConfirmed = true;
            member.ConfirmEmailId = string.Empty;
            TableOperation tableOperation = TableOperation.Replace(member);
            table.Execute(tableOperation);

            return Request.CreateResponse(HttpStatusCode.OK, member.Email + " has been confirmed.");
        }
    }
}
