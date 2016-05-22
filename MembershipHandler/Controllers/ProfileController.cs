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
    public class ProfileController : BaseFacebookController
    {
        [HttpPost]
        public Profile Post()
        {
            if (!CurrentUser.Confirmed)
            {
                ConfirmUserIfStudentOrOverHalf();
            }
            Profile response = new Profile();

            response.Name = CurrentUser.Name;            
            response.StudentId = CurrentUser.StudentId;
            response.EmailAddress = CurrentUser.EmailAddress;

            if (!CurrentUser.Confirmed)
            {
                response.MembershipType = "Pending";
            }
            else if (CurrentUser.Lifetime)
            {
                response.MembershipType = "Lifetime";
            }
            else if (CurrentUser.Premium)
            {
                response.MembershipType = "Premium";
            }
            else
            {
                response.MembershipType = "Standard";
            }

            if (CurrentUser.Committee)
            {
                CloudTable committeeTable = TableClient.GetTableReference("Committee");
                committeeTable.CreateIfNotExists();
                TableOperation retrieveOperation = TableOperation.Retrieve<Committee>("Committee", CurrentUser.RowKey);                
                Committee committee = (Committee)committeeTable.Execute(retrieveOperation).Result;

                if (committee != null)
                {
                    response.CommitteeTitle = committee.Title;
                }
            }
            else
            {
                response.CommitteeTitle = null;
            }

            return response;
        }

        [NonAction]
        private void ConfirmUserIfStudentOrOverHalf()
        {
            if ((CurrentUser.StudentId != null && CurrentUser.StudentId != string.Empty)
                || HalfMembersAreStudents())
            {
                CurrentUser.Confirmed = true;
                CloudTable memberTable = TableClient.GetTableReference("Members");
                TableOperation tableOperation = TableOperation.Replace(CurrentUser);
                memberTable.Execute(tableOperation);
            }
        }
    }
}
