using MembershipHandler.Filters;
using MembershipHandler.Models;
using Microsoft.Azure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace MembershipHandler.Controllers
{
    public class SlackInviteController : BaseController
    {
        [HttpGet]
        [AllowCrossSiteOrigin]
        [Obsolete]
        public HttpResponseMessage Get(string id)
        {
            if (id.Length != Guid.NewGuid().ToString().Length)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, " ");
            }

            CloudTable table = tableClient.GetTableReference("Members");
            table.CreateIfNotExists();
            
            TableQuery<Member> matchingAccount = new TableQuery<Member>().Where(
                    TableQuery.GenerateFilterCondition("ConfirmEmailId", QueryComparisons.Equal, id));
            Member member = table.ExecuteQuery(matchingAccount).FirstOrDefault();
            if (member == null)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, " ");
            }

            member.EmailConfirmed = true;
            member.ConfirmEmailId = string.Empty;
            TableOperation tableOperation = TableOperation.Replace(member);
            table.Execute(tableOperation);

            // using slack api (undocumented method documented here: https://github.com/ErikKalkoken/slackApiDoc)
            var client = new RestClient("https://aucs.slack.com/api/");
            // client.Authenticator = new HttpBasicAuthenticator(username, password);

            var request = new RestRequest("users.admin.invite", Method.GET);
            request.AddParameter("token", CloudConfigurationManager.GetSetting("SlackAuthenticationToken")); // Authentication token (Requires scope: ??)
            request.AddParameter("email", member.Email);

            Tuple<string, string> brokenName = BreakName(member.Name);
            request.AddParameter("first_name", brokenName.Item1);
            if (brokenName.Item2 != string.Empty)
            {
                request.AddParameter("last_name", brokenName.Item2);
            }

            // execute the request
            IRestResponse response = client.Execute(request);

            if (response.Content.Contains("already_in_team"))
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                    "Thanks, your email address has been confirmed, and you are already in the slack team.");
            }
            else if (!response.Content.Contains("error"))
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                    "Thanks, your email address has been confirmed. Unfortunately there has been an error with slackr. Please report this to a committee member: " + response.Content);
            }
            else
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                    "Thanks, your email address has been confirmed and a slack invite has been sent to you.");
            }            
        }

        [NonAction]
        private Tuple<string, string> BreakName(string fullName)
        {
            if (!fullName.Contains(' '))
            {
                return new Tuple<string, string>(fullName, string.Empty);
            }
            
            int nameSpace = fullName.IndexOf(' ');
            string firstName = fullName.Substring(0, nameSpace);
            string lastName = fullName.Substring(nameSpace + 1, fullName.Length - nameSpace - 1);

            if ((firstName != null && firstName != string.Empty)
                && (lastName != null && lastName != string.Empty))
            {
                return new Tuple<string, string>(firstName, lastName);
            }
            else
            {
                return new Tuple<string, string>(fullName, string.Empty);
            }
        }
    }
}
