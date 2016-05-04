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
    public class SlackInviteController : ApiController
    {
        [HttpGet]
        [AllowCrossSiteOrigin]
        public HttpResponseMessage Get(string id)
        {
            RegisterController.RemoveOldUnconfirmedAccounts();

            HttpStatusCode resultEmail = EmailConfirmController.ConfirmEmail(id);

            if (resultEmail == HttpStatusCode.OK)
            {
                string result = "Thanks, your email address has been confirmed";
                string resultSlack = SendSlackInvitation(id);

                if (resultSlack.Contains("already_in_team"))
                {
                    result += ", but you are already in the slack team.";
                }
                else if (!resultSlack.Contains("error"))
                {
                    result += " and a slack invite has been sent to you.";
                }
                else
                {
                    result += ". Unfortunately there has been a slack error. Please report this to a committee member: " + resultSlack;
                }

                return Request.CreateResponse(HttpStatusCode.OK, result);
            }
            return Request.CreateResponse(HttpStatusCode.BadRequest, " ");
        }

        [NonAction]
        private string SendSlackInvitation(string guid)
        {
            // Retrieve the storage account from the connection string.
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(
                CloudConfigurationManager.GetSetting("StorageConnectionString"));
            // Create the table client.
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();
            // Retrieve a reference to the table.
            CloudTable table = tableClient.GetTableReference("Members");
            // Create the table if it doesn't exist.
            table.CreateIfNotExists();
            // Create the table query.
            TableQuery<Member> emailExistsQuery = new TableQuery<Member>().Where(
                    TableQuery.GenerateFilterCondition("ConfirmEmailId", QueryComparisons.Equal, guid));
            List<Member> results = table.ExecuteQuery(emailExistsQuery).ToList();
            if (results.Count < 1)
            {
                return "Unsolvable error";
            }

            Member member = results[0];

            // using slack api (undocumented method documented here: https://github.com/ErikKalkoken/slackApiDoc)
            var client = new RestClient("https://aucs.slack.com/api/");
            // client.Authenticator = new HttpBasicAuthenticator(username, password);

            var request = new RestRequest("users.admin.invite", Method.GET);
            request.AddParameter("token", CloudConfigurationManager.GetSetting("SlackAuthenticationToken")); // Authentication token (Requires scope: ??)
            request.AddParameter("email", member.Email);
            
            if (member.Name.Contains(' '))
            {
                int nameSpace = member.Name.IndexOf(' ');
                string firstName = member.Name.Substring(0, nameSpace);
                string lastName = member.Name.Substring(nameSpace + 1, member.Name.Length - nameSpace - 1);
                if (firstName != null && firstName != string.Empty)
                {
                    request.AddParameter("first_name", firstName);                
                }
                if (lastName != null && lastName != string.Empty)
                {
                    request.AddParameter("last_name", lastName);                
                }   
            }

            // execute the request
            IRestResponse response = client.Execute(request);
            return response.Content; // raw content as string
        }
    }
}
