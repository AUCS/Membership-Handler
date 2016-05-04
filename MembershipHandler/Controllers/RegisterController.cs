using MembershipHandler.Models;
using Microsoft.Azure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using SendGrid;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Mail;
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
        [AllowCrossSiteOrigin]
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
            TableQuery<Member> query = new TableQuery<Member>().Where(
                    TableQuery.GenerateFilterCondition("Email", QueryComparisons.Equal, form.Email.ToLowerInvariant()));
            List<Member> results = table.ExecuteQuery(query).ToList();
            TableOperation tableOperation;
            if (results.Count > 0)
            {
                if (results[0].EmailConfirmed)
                {
                    if (!results[0].StudentConfirmed && form.StudentId != null)
                    {
                        if (!CheckGoodStudentId(form.StudentId))
                        {
                            return Request.CreateResponse(HttpStatusCode.Conflict, "That Student Id already has an email associated with it.");
                        }
                        results[0].StudentId = form.StudentId.ToLowerInvariant();
                        results[0].ConfirmStudentId = Guid.NewGuid().ToString();
                        // Create the TableOperation object that inserts the customer entity.
                        tableOperation = TableOperation.Replace(results[0]);
                        // Execute the insert operation.
                        table.Execute(tableOperation);
                        SendStudentEmail(results[0]);
                        return Request.CreateResponse(HttpStatusCode.OK, "Thanks " + results[0].Name + ". We've sent you an email to confirm your Student Id.");
                    }
                    return Request.CreateResponse(HttpStatusCode.Conflict, "Already a member");
                }
                SendEmail(results[0]);
                return Request.CreateResponse(HttpStatusCode.OK, "We've sent you another email. Please check your junk folder.");
            }

            Member newMember = new Member();
            newMember.Email = form.Email.ToLowerInvariant();
            newMember.Name = form.Name;
            newMember.ConfirmEmailId = Guid.NewGuid().ToString();
            newMember.RegistrationDate = DateTime.UtcNow;
            if (form.StudentId != null)
            {
                if (!CheckGoodStudentId(form.StudentId))
                {
                    return Request.CreateResponse(HttpStatusCode.Conflict, "That Student Id already has an email associated with it.");
                }
                newMember.StudentId = form.StudentId.ToLowerInvariant();
                newMember.ConfirmStudentId = Guid.NewGuid().ToString();
                SendStudentEmail(newMember);
            }
            else
            {
                newMember.StudentConfirmed = false;
                query = new TableQuery<Member>().Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.GreaterThanOrEqual, "2016"));
                results = table.ExecuteQuery(query).ToList();
                int students = results.Where(q => q.StudentConfirmed).Count();
                int nonStudents = results.Where(q => !q.StudentConfirmed).Count();
                if (nonStudents >= students)
                {
                    return Request.CreateResponse(HttpStatusCode.Conflict, "Unfortunately we are not taking any new members that are not Adelaide Uni Students at this time.");
                }
            }
            // Create the TableOperation object that inserts the customer entity.
            tableOperation = TableOperation.Insert(newMember);
            // Execute the insert operation.
            table.Execute(tableOperation);
            SendEmail(newMember);
            return Request.CreateResponse(HttpStatusCode.OK, "Thanks " + newMember.Name + ". We've sent you an email to confirm your email address.");
        }

        [NonAction]
        private bool CheckGoodStudentId(string id)
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

            TableQuery<Member> query = new TableQuery<Member>().Where(
                    TableQuery.CombineFilters(
                        TableQuery.GenerateFilterConditionForBool("StudentConfirmed", QueryComparisons.Equal, true),
                        TableOperators.And,
                        TableQuery.GenerateFilterCondition("StudentId", QueryComparisons.Equal, id.ToLowerInvariant()))
                    );
            List<Member> results = table.ExecuteQuery(query).ToList();
            if (results.Count > 0)
            {
                return false;
            }
            return true;
        }

        [NonAction]
        private void SendEmail(Member member)
        {
            // Email needs to account for new members, and old members adding
            // uni ids, or just wanting another chance at joining slack

            // options are:
            //      confirm email and send slack invite,
            //      confirm email without slack invite
            //      do nothing and will be removed after 48 hours+
            // Create the email object first, then add the properties.
            SendGridMessage myMessage = new SendGridMessage();
            myMessage.AddTo(member.Email);
            myMessage.From = new MailAddress("membership@aucs.club", "Adelaide Uni Cheese Society");
            myMessage.Subject = "Welcome to the AUCS!";
            myMessage.Text = Emails.RegisterEmail.Text;
            myMessage.Text = myMessage.Text.Replace("<name>", member.Name);
            myMessage.Text = myMessage.Text.Replace("<emailid>", member.ConfirmEmailId);

            // Create a Web transport, using API Key
            // Retrieve the storage account from the connection string.
            var transportWeb = new Web(CloudConfigurationManager.GetSetting("SendGridAPIKey"));

            // Send the email.
            transportWeb.DeliverAsync(myMessage);
        }

        [NonAction]
        private void SendStudentEmail(Member member)
        {
            if (member.Email == member.StudentId + "@student.adelaide.edu.au"
                || member.Email == member.StudentId + "@adelaide.edu.au")
            {
                // Email is uni email don't send a second confirmation
                return;
            }
            SendGridMessage myMessage = new SendGridMessage();
            myMessage.AddTo(member.StudentId + "@student.adelaide.edu.au");
            myMessage.AddTo(member.StudentId + "@adelaide.edu.au");
            myMessage.From = new MailAddress("membership@aucs.club", "Adelaide Uni Cheese Society");
            myMessage.Subject = "Welcome to the AUCS!";
            myMessage.Text = Emails.ConfirmStudentEmail.Text;
            myMessage.Text = myMessage.Text.Replace("<name>", member.Name);
            myMessage.Text = myMessage.Text.Replace("<studentid>", member.ConfirmStudentId);

            // Create a Web transport, using API Key
            // Retrieve the storage account from the connection string.
            var transportWeb = new Web(CloudConfigurationManager.GetSetting("SendGridAPIKey"));

            // Send the email.
            transportWeb.DeliverAsync(myMessage);
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
            DateTimeOffset hours48 = DateTime.UtcNow.AddHours(-48); 
            TableQuery<Member> rangeQuery = new TableQuery<Member>().Where(
                TableQuery.CombineFilters(
                    TableQuery.GenerateFilterConditionForDate("RegistrationDate", QueryComparisons.LessThan, hours48),
                    TableOperators.And, TableQuery.CombineFilters(
                        TableQuery.GenerateFilterConditionForBool("EmailConfirmed", QueryComparisons.Equal, false),
                        TableOperators.Or,
                        TableQuery.GenerateFilterConditionForBool("StudentConfirmed", QueryComparisons.Equal, false)
                    )
                ));

            List<Member> results = table.ExecuteQuery(rangeQuery).ToList();
            // Create the batch operation.
            if (results != null)
            {
                TableBatchOperation batchOperation = new TableBatchOperation();
                for (int i = 0; i < results.Count; i++)
                {
                    if (!results[i].EmailConfirmed)
                    {
                        batchOperation.Delete(results[i]);
                    }
                    else if (!results[i].StudentConfirmed)
                    {
                        results[i].StudentId = null;
                        results[i].ConfirmStudentId = null;
                        batchOperation.Replace(results[i]);
                    }
                }
                // Execute the batch operation.
                if (batchOperation.Count > 0)
                {
                    table.ExecuteBatch(batchOperation);
                }
            }
        }
    }
}
