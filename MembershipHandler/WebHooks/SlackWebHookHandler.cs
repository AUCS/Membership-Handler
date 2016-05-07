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
using RestSharp;

namespace MembershipHandler.WebHooks
{
    public class SlackWebHookHandler : BaseWebHookHandler
    {
        public SlackWebHookHandler()
        {
            Receiver = "Slack";
        }

        public override Task ExecuteAsync(string receiver, WebHookHandlerContext context)
        {
            NameValueCollection command = context.GetDataOrDefault<NameValueCollection>();
            string slackId = command["user_id"];
            
            KeyValuePair <string, string> slashCommand = SlackCommand.ParseActionWithValue(command["text"]);
            if (slashCommand.Key == null || slashCommand.Key == string.Empty)
            {
                // No commands given, send help
                return Task.FromResult(SendHowTo(context, null));
            }

            switch (slashCommand.Key)
            {
                case "view":
                    return Task.FromResult(ShowMembershipDetails(context, slackId));
                case "studentid":
                    return Task.FromResult(UpdateStudentId(context, slackId, slashCommand.Value));
                case "premium":
                    return Task.FromResult(UpdatePremium(context, slackId));
                case "committee":
                    // Todo: make people who want to be committee members register themselves here maybe?
                default:
                    return Task.FromResult(SendHowTo(context, slashCommand.Key));
            }
        }

        

        private bool ShowMembershipDetails(WebHookHandlerContext context, string slackId)
        {
            Member member = GetMember(slackId);
            if (member == null)
            {
                context.Response = context.Request.CreateResponse(
                    new SlackSlashResponse("A bad error occured with the slack webhook, code: 1. Please contact a Committee Member about this."));
                return true;
            }

            SlackSlashResponse slashReply = new SlackSlashResponse("Your AUCS Membership Details:");
            SlackAttachment att = new SlackAttachment(member.Name, member.Name);
            att.Fields.Add(new SlackField("Email", member.Email));

            if (member.IsCommittee)
            {
                att.Title = GetCommitteeTitle(member.AUCSID);
            }
            if (member.StudentConfirmed)
            {
                att.Fields.Add(new SlackField("Student Id", member.StudentId));
            }
            
            slashReply.Attachments.Add(att);
            context.Response = context.Request.CreateResponse(slashReply);
            return true;
        }

        private bool UpdateStudentId(WebHookHandlerContext context, string slackId, string studentId)
        {
            string newStudentId = studentId;
            if (!StudentIdAttribute.IsStudentId(newStudentId))
            {
                context.Response = context.Request.CreateResponse(
                    new SlackSlashResponse("\"" + newStudentId + "\" is not a valid Student Id."));
                return true;
            }

            Member member = GetMember(slackId);
            if (member == null)
            {
                context.Response = context.Request.CreateResponse(
                    new SlackSlashResponse("A bad error occured with the slack webhook, code: 1. Please contact a Committee Member about this."));
                return true;
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
            tableClient.GetTableReference("Members").Execute(tableOperation);
            Emails.EmailHandler.SendStudentConfirm(member);

            context.Response = context.Request.CreateResponse(
                new SlackSlashResponse("An email has been sent to " + member.StudentId + "@student.adelaide.edu.au to confirm this change."));
            return true;
        }

        private bool UpdatePremium(WebHookHandlerContext context, string slackId)
        {
            SlackSlashResponse slashReply = new SlackSlashResponse("tbd..");
            context.Response = context.Request.CreateResponse(slashReply);
            return true;
        }

        private bool SendHowTo(WebHookHandlerContext context, string failedCommand)
        {
            var slashReply = new SlackSlashResponse("View and Update your AUCS Membership Details");
            var att = new SlackAttachment("Possible Options:", "Possible Options:");

            if (failedCommand != null)
            {
                att.Pretext = "'" + failedCommand + "' is not a valid option.";
            }

            // Slash attachments can contain tabular data as well
            att.Fields.Add(new SlackField("View Your Membership Details:",
                "/membership view"));
            att.Fields.Add(new SlackField("Update your Student Id:",
                "/membership studentid [new-student-id]"));
            att.Fields.Add(new SlackField("Update your Premium Status:",
                "/membership premium"));

            // A reply can contain multiple attachments
            slashReply.Attachments.Add(att);

            // Return slash command response
            context.Response = context.Request.CreateResponse(slashReply);

            return true;
        }




        private Member GetMember(string slackId)
        {
            CloudTable table = tableClient.GetTableReference("Members");
            table.CreateIfNotExists();
            TableQuery<Member> results = new TableQuery<Member>().Where(
                    TableQuery.GenerateFilterCondition("SlackId", QueryComparisons.Equal, slackId));
            Member member = table.ExecuteQuery(results).FirstOrDefault();
            if (member == null)
            {
                return null;
            }
            return member;
        }
    }
}