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
    public class StudentChangeController : BaseController
    {
        [HttpPost]
        public HttpResponseMessage Post(SlackSlash slack)
        {
            if (!ModelState.IsValid || (slack.token != CloudConfigurationManager.GetSetting("SlackChangeStudentIdToken")))
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, "Invalid parameters");
            }
            if (!StudentIdAttribute.IsStudentId(slack.text))
            {
                return Request.CreateResponse(HttpStatusCode.OK, '"' + slack.text + "' is not a valid Student Id.");
            }

            CloudTable table = tableClient.GetTableReference("Members");
            table.CreateIfNotExists();
            TableQuery<Member> results = new TableQuery<Member>().Where(
                    TableQuery.GenerateFilterCondition("SlackId", QueryComparisons.Equal, slack.user_id));
            Member member = table.ExecuteQuery(results).FirstOrDefault();
            
            if (member == null)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, "Bad Token");
            }

            if (slack.text == null || slack.text == string.Empty)
            {
                if (member.StudentConfirmed)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, "Your Student Id is: " + member.StudentId + ".");
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.OK, "You do not currently have a confirmed Student Id.");
                }
            }


            member.StudentConfirmed = false;
            member.StudentId = slack.text;

            TableOperation tableOperation = TableOperation.Replace(member);
            table.Execute(tableOperation);
            SendStudentConfirm(member);

            return Request.CreateResponse(HttpStatusCode.OK, "An email has been sent to " + member.StudentId + "@student.adelaide.edu.au to confirm this change.");
        }
    }
}
