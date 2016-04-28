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
    public class RegisterController : ApiController
    {
        [HttpGet]
        public string Get(int id)
        {
            RegisterController.RemoveOldUnconfirmedAccounts();
            return "This server is on";
        }

        // POST api/member
        [HttpPost]
        public HttpResponseMessage Post(Register form)
        {
            if (!ModelState.IsValid)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, "Invalid parameters.");
            }

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
                    TableQuery.GenerateFilterCondition("Email", QueryComparisons.Equal, form.Email));
            List<Member> results = table.ExecuteQuery(emailExistsQuery).ToList();
            if (results.Count > 0)
            {
                SendEmail(results[0]);
                if (results[0].EmailConfirmed)
                {
                    return Request.CreateResponse(HttpStatusCode.Conflict, "Already a member");
                }
                return Request.CreateResponse(HttpStatusCode.OK, "We've sent you another email.");
            }

            Member newMember = new Member();
            newMember.Email = form.Email;
            newMember.Name = form.Name;
            newMember.EmailId = Guid.NewGuid().ToString();
            newMember.RegistrationDate = DateTime.UtcNow;
            if (form.StudentId != null)
            {
                newMember.IsAdelaideUniStudent = true;
                newMember.AdelaideUniStudentId = form.StudentId;
            }
            // Create the TableOperation object that inserts the customer entity.
            TableOperation tableOperation = TableOperation.Insert(newMember);
            // Execute the insert operation.
            table.Execute(tableOperation);
            SendEmail(newMember);
            return Request.CreateResponse(HttpStatusCode.OK, "Thanks " + newMember.Name + ".\nWe've sent you an email to confirm your email address.");
        }

        [NonAction]
        private void SendEmail(Member member)
        {
            // Email needs to account for new members, and old members adding
            // uni ids, or just wanting another chance at joining slack

            // options are:
            //      confirm email and send slack invite,
            //      confirm email without slack invite
            //      do nothing and will be removed after 48 hours+ (needs to be implemented)
        }

        [NonAction]
        public static void RemoveOldUnconfirmedAccounts()
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
            DateTime hours48 = DateTime.UtcNow.AddHours(-48); 
            TableQuery<Member> rangeQuery = new TableQuery<Member>().Where(
                TableQuery.CombineFilters(
                    TableQuery.GenerateFilterCondition("EmailConfirmed", QueryComparisons.Equal, false.ToString()),
                    TableOperators.And,
                    TableQuery.GenerateFilterCondition("RegistrationDate", QueryComparisons.LessThan, hours48.ToString())));

            List<Member> results = table.ExecuteQuery(rangeQuery).ToList();
            // Create the batch operation.
            TableBatchOperation batchOperation = new TableBatchOperation();
            for (int i = 0; i < results.Count; i++)
            {
                batchOperation.Delete(results[i]);
            }
            // Execute the batch operation.
            table.ExecuteBatch(batchOperation);
        }
    }
}
