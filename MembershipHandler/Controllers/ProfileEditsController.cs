using MembershipHandler.Models;
using MembershipHandler.Views;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace MembershipHandler.Controllers
{
    public class ProfileEditsController : FacebookLoginController
    {
        [HttpPost]
        public string Post(Edits edits)
        {
            string result = null;
            if (edits.Email != null)
            { 
                if (!ModelState.IsValid)
                {
                    return edits.Email + " is an invalid email address.";
                }

                edits.Email = edits.Email.ToLowerInvariant();
                Confirm confirm = new Confirm("Email");
                confirm.RowKey = CurrentUser.RowKey;
                confirm.Value = edits.Email;
                
                CloudTable table = TableClient.GetTableReference("Confirms");
                table.CreateIfNotExists(); // insert or replace as the user can only have one email confirm
                TableOperation tableOperation = TableOperation.InsertOrReplace(confirm);
                table.Execute(tableOperation);
                Emails.EmailHandler.SendEmailConfirm(CurrentUser, confirm);

                result = edits.Email;
            }
            if (edits.StudentId != null)
            {
                if (!Filters.StudentIdAttribute.IsStudentId(edits.StudentId))
                {
                    return edits.StudentId + " is an invalid student id.";
                }

                edits.StudentId = edits.StudentId.ToLowerInvariant();
                Confirm confirm = new Confirm("StudentId");
                confirm.RowKey = CurrentUser.RowKey;
                confirm.Value = edits.StudentId;

                CloudTable table = TableClient.GetTableReference("Confirms");
                table.CreateIfNotExists();  // insert or replace as the user can only have one student id confirm
                TableOperation tableOperation = TableOperation.InsertOrReplace(confirm);
                table.Execute(tableOperation);
                Emails.EmailHandler.SendStudentConfirm(CurrentUser, confirm);

                if (result != null)
                {
                    result += " and ";
                }
                result += edits.StudentId + "@student.adelaide.edu.au";
            }
            return "We have sent an email to " + result + " to confirm. Remember to check your spam/junk folders.";
        }
    }
}
