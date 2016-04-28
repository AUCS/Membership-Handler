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
                if (results[0].EmailConfirmed)
                {
                    return Request.CreateResponse(HttpStatusCode.Conflict, "Already a member");
                }
                SendConfirmEmail(results[0]);
                return Request.CreateResponse(HttpStatusCode.OK, "We've sent you another email.");
            }

            Member newMember = new Member();
            newMember.Email = form.Email;
            newMember.Name = form.Name;
            if (form.StudentId != null)
            {
                newMember.IsAdelaideUniStudent = true;
                newMember.AdelaideUniStudentId = form.StudentId;
            }
            newMember.ConfirmEmailId = Guid.NewGuid().ToString();
            // Create the TableOperation object that inserts the customer entity.
            TableOperation tableOperation = TableOperation.Insert(newMember);
            // Execute the insert operation.
            table.Execute(tableOperation);
            SendConfirmEmail(newMember);
            return Request.CreateResponse(HttpStatusCode.OK, "Thanks " + newMember.Name + ".\nWe've sent you an email to confirm your email address.");
        }

        [NonAction]
        private void SendConfirmEmail(Member member)
        {

        }
    }
}
