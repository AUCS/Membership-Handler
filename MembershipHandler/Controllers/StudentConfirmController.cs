using MembershipHandler.Filters;
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
    public class StudentConfirmController : BaseController
    {
        [HttpGet]
        [AllowCrossSiteOrigin]
        public HttpResponseMessage Get(string id)
        {
            if (id.Length != Guid.NewGuid().ToString().Length)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, " ");
            }

            CloudTable table = tableClient.GetTableReference("Members");
            table.CreateIfNotExists();

            TableQuery<Member> matchingAccount = new TableQuery<Member>().Where(
                    TableQuery.GenerateFilterCondition("ConfirmStudentId", QueryComparisons.Equal, id));
            Member member = table.ExecuteQuery(matchingAccount).FirstOrDefault();
            if (member == null)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, " ");
            }

            member.StudentConfirmed = true;
            member.ConfirmStudentId = string.Empty;
            TableOperation tableOperation = TableOperation.Replace(member);
            table.Execute(tableOperation);

            return Request.CreateResponse(HttpStatusCode.OK, "Thanks, your Student Id has been confirmed.");
        }
    }
}
