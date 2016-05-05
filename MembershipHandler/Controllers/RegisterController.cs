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
                if (ModelState.Keys.Contains("form.StudentId"))
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Invalid Student Id.");
                }
                return Request.CreateResponse(HttpStatusCode.BadRequest, "Invalid parameters.");
            }
            
            CloudTable table = tableClient.GetTableReference("Members");
            table.CreateIfNotExists();
            TableQuery<Member> clashQuery;

            form.Name = form.Name.Trim();
            form.Email = form.Email.ToLowerInvariant();
            if (form.StudentId != null)
            {
                form.StudentId = form.StudentId.Trim();
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

            Member newMember;
            if (results.Any(q => q.Email == form.Email))
            {   // Email already in but unconfirmed, assume change of name or student id
                newMember = results.First(q => q.Email == form.Email);
            }
            else
            {   // New Email and therefore new user
                newMember = new Member();
                newMember.Email = form.Email;
            }
            newMember.Name = form.Name;
            newMember.ConfirmEmailId = Guid.NewGuid().ToString();
            newMember.RegistrationDate = DateTime.UtcNow;

            if (form.StudentId != null)
            {
                newMember.StudentId = form.StudentId;
                newMember.ConfirmStudentId = Guid.NewGuid().ToString();
            }

            TableOperation tableOperation = TableOperation.InsertOrReplace(newMember);
            table.Execute(tableOperation);
            
            Emails.EmailHandler.SendEmailConfirm(newMember);
            string note = "Thanks, " + newMember.Name + "! We've sent you an email with more information and the next steps you need to take.";
            if (newMember.StudentId != null)
            {
                Emails.EmailHandler.SendStudentConfirm(newMember);
                note = "Thanks, " + newMember.Name + "! We've sent you two emails, one with more information and the next steps you need to take, and another to your Student Id to confirm that it's yours.";
            }

            return Request.CreateResponse(HttpStatusCode.OK, note);
        }
    }
}
