using Facebook;
using MembershipHandler.Filters;
using MembershipHandler.Models;
using Microsoft.Azure;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace MembershipHandler.Controllers
{
    public class LoginController : FacebookLoginController
    {
        [HttpPost]
        public string Post()
        {
            if (UserFBId == null)
            {
                return "not_logged_in_facebook";
            }

            if (CurrentUser == null)
            {
                if (!HalfMembersAreStudents())
                {
                    CreateNewUser(false);
                    return "not_enough_students";
                }
                CreateNewUser(true);
            }
            return "current";
        }

        [NonAction]
        private  void CreateNewUser(bool confirmed)
        {
            Member newMember = new Member();
            newMember.RowKey = UserFBId;
            newMember.Confirmed = confirmed;

            dynamic fbuser = FBClient.Get(UserFBId);
            newMember.Name = fbuser.name;

            CloudTable table = TableClient.GetTableReference("Members");
            TableOperation tableOperation = TableOperation.InsertOrReplace(newMember);
            table.Execute(tableOperation);
        }
    }
}
