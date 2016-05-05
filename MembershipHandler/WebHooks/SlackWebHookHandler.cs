using Microsoft.AspNet.WebHooks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Threading.Tasks;
using System.Collections.Specialized;
using System.Net.Http;
using MembershipHandler.Filters;
using System.Net;
using Microsoft.WindowsAzure.Storage.Table;
using MembershipHandler.Models;

namespace MembershipHandler.WebHooks
{
    public class SlackWebHookHandler : BaseWebHookHandler
    {
        public SlackWebHookHandler()
        {
            Receiver = SlackWebHookReceiver.ReceiverName;
        }

        public override Task ExecuteAsync(string receiver, WebHookHandlerContext context)
        {
            NameValueCollection command = context.GetDataOrDefault<NameValueCollection>();

            switch(context.Id)
            {
                case "updateid":
                    return Task.FromResult(UpdateStudentId(context, command));
                case "other":
                    break;
            }

            return Task.FromResult(false);
        }

        private bool UpdateStudentId(WebHookHandlerContext context, NameValueCollection command)
        {
            string newStudentId = command["text"];
            if (!StudentIdAttribute.IsStudentId(newStudentId))
            {
                context.Response = context.Request.CreateResponse(
                    new SlackSlashResponse("\"" + newStudentId + "\" is not a valid Student Id."));
                return true;
            }

            CloudTable table = tableClient.GetTableReference("Members");
            table.CreateIfNotExists();
            TableQuery<Member> results = new TableQuery<Member>().Where(
                    TableQuery.GenerateFilterCondition("SlackId", QueryComparisons.Equal, command["user_id"]));
            Member member = table.ExecuteQuery(results).FirstOrDefault();
            if (member == null)
            {
                return false;
            }

            if (newStudentId  == null || newStudentId == string.Empty)
            {
                if (member.StudentConfirmed)
                {
                    context.Response = context.Request.CreateResponse(
                        new SlackSlashResponse("Your Student Id is: " + member.StudentId + "."));
                    return true;
                }
                else
                {
                    context.Response = context.Request.CreateResponse(
                        new SlackSlashResponse("You do not currently have a confirmed Student Id."));
                    return true;
                }
            }


            member.StudentConfirmed = false;
            member.StudentId = newStudentId;

            TableOperation tableOperation = TableOperation.Replace(member);
            table.Execute(tableOperation);
            Emails.EmailHandler.SendStudentConfirm(member);

            context.Response = context.Request.CreateResponse(
                new SlackSlashResponse("An email has been sent to " + member.StudentId + "@student.adelaide.edu.au to confirm this change."));
            return true;
        }
    }
}