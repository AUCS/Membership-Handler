using MembershipHandler.Filters;
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
    public class RegisterController : BaseController
    {
        [HttpGet]
        public string Get(int id)
        {
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
            
            CloudTable table = tableClient.GetTableReference("Members");
            table.CreateIfNotExists();
            TableQuery<Member> clashQuery;

            form.Email = form.Email.ToLowerInvariant();
            if (form.StudentId != null)
            {
                form.StudentId = form.StudentId.ToLowerInvariant();
                clashQuery = new TableQuery<Member>().Where(
                    TableQuery.CombineFilters(
                        TableQuery.GenerateFilterCondition("Email", QueryComparisons.Equal, form.Email),
                        TableOperators.Or,
                        TableQuery.GenerateFilterCondition("StudentId", QueryComparisons.Equal, form.StudentId)
                    ));
            }
            else
            {
                if (!HalfMembersAreStudents())
                {
                    return Request.CreateResponse(HttpStatusCode.Conflict, "Unfortunately we are not taking any new members that are not members of Adelaide University at this time.");
                }
                clashQuery = new TableQuery<Member>().Where(
                    TableQuery.GenerateFilterCondition("Email", QueryComparisons.Equal, form.Email));
            }

            List<Member> results = table.ExecuteQuery(clashQuery).ToList();
            if (results.Any(q => q.StudentConfirmed && q.StudentId == form.StudentId))
            {   // Student Id Clashes
                return Request.CreateResponse(HttpStatusCode.Conflict,
                    "That student id already has an email associated with it.");
            }
            if (results.Any(q => q.EmailConfirmed && q.Email == form.Email))
            {   // Email address Clashes
                return Request.CreateResponse(HttpStatusCode.Conflict,
                    "That email address is already in use.");
            }

            Member newMember = new Member();
            newMember.Email = form.Email;
            newMember.Name = form.Name;
            newMember.ConfirmEmailId = Guid.NewGuid().ToString();
            newMember.RegistrationDate = DateTime.UtcNow;
            if (form.StudentId != null)
            {
                newMember.StudentId = form.StudentId;
                newMember.ConfirmStudentId = Guid.NewGuid().ToString();
            }
            TableOperation tableOperation = TableOperation.Insert(newMember);
            table.Execute(tableOperation);
            
            SendEmailConfirm(newMember);
            string note = "Thanks, " + newMember.Name + "! We've sent you an email with more information and the next steps you need to take.";
            if (newMember.StudentId != null)
            {
                SendStudentConfirm(newMember);
                note = "Thanks, " + newMember.Name + "! We've sent you two emails, one with more information and the next steps you need to take, and another to your Student Id to confirm that it's yours.";
            }

            return Request.CreateResponse(HttpStatusCode.OK, note);
        }

        [NonAction]
        private bool HalfMembersAreStudents()
        {            
            CloudTable table = tableClient.GetTableReference("Members");
            table.CreateIfNotExists();

            TableQuery<Member> query = new TableQuery<Member>().Select(new List<string> { "StudentConfirmed" });
            List<bool> results = table.ExecuteQuery(query).Select(q => q.StudentConfirmed).ToList();

            int students = results.Count(q => q);
            int nonStudents = results.Count(q => !q);
            if (students > nonStudents)
            {
                return true;
            }
            return false;
        }

        [NonAction]
        private void SendEmailConfirm(Member member)
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
            myMessage.Text = Emails.ConfirmEmail.Text;
            myMessage.Text = myMessage.Text.Replace("<name>", member.Name);
            myMessage.Text = myMessage.Text.Replace("<emailid>", member.ConfirmEmailId);

            // Create a Web transport, using API Key
            // Retrieve the storage account from the connection string.
            var transportWeb = new Web(CloudConfigurationManager.GetSetting("SendGridAPIKey"));

            // Send the email.
            transportWeb.DeliverAsync(myMessage);
        }

        [NonAction]
        private void SendStudentConfirm(Member member)
        {
            SendGridMessage myMessage = new SendGridMessage();
            myMessage.AddTo(member.StudentId + "@student.adelaide.edu.au");
            myMessage.From = new MailAddress("membership@aucs.club", "Adelaide Uni Cheese Society");
            myMessage.Subject = "Confirm your Student Id";
            myMessage.Text = Emails.ConfirmStudent.Text;
            myMessage.Text = myMessage.Text.Replace("<name>", member.Name);
            myMessage.Text = myMessage.Text.Replace("<studentid>", member.StudentId);
            myMessage.Text = myMessage.Text.Replace("<email>", member.Email);
            myMessage.Text = myMessage.Text.Replace("<linkid>", member.ConfirmStudentId);

            // Create a Web transport, using API Key
            // Retrieve the storage account from the connection string.
            var transportWeb = new Web(CloudConfigurationManager.GetSetting("SendGridAPIKey"));

            // Send the email.
            transportWeb.DeliverAsync(myMessage);
        }
    }
}
