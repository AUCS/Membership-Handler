using Microsoft.Azure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Controllers;
using MembershipHandler.Models;
using SendGrid;
using System.Net.Mail;
using Facebook;
using MembershipHandler.Filters;

namespace MembershipHandler.Controllers
{
    public class FacebookLoginController : ApiController
    {
        private CloudStorageAccount storageAccount;
        protected CloudTableClient TableClient;
        protected FacebookClient FBClient;

        protected const string AUCSFBId = "1549671362000387";
        protected string UserFBId;
        protected Member CurrentUser;
        
        public override Task<HttpResponseMessage> ExecuteAsync(HttpControllerContext controllerContext, CancellationToken cancellationToken)
        {
            // Retrieve the storage account from the connection string.
            storageAccount = CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting("StorageConnectionString"));
            // Create the table client.
            TableClient = storageAccount.CreateCloudTableClient();

            //Setup facebook client
            FBClient = new FacebookClient();
            dynamic accessToken = FBClient.Get("oauth/access_token", new
            {
                client_id = "631831156964129",
                client_secret = CloudConfigurationManager.GetSetting("FacebookAUCSAppSecret"),
                grant_type = "client_credentials"
            });
            FBClient.AccessToken = accessToken.access_token;

            CurrentUser = null;
            if (controllerContext.Request.Headers.Contains("Token"))
            {
                string token = controllerContext.Request.Headers.GetValues("Token").FirstOrDefault();
                if (token != null && token != string.Empty)
                {
                    FacebookClient userClient = new FacebookClient(token);
                    dynamic user = userClient.Get("me");
                    if (user != null && user.id != null && user.id != string.Empty)
                    {
                        UserFBId = (string)user.id;
                        CurrentUser = GetMember((string)user.id);
                        return base.ExecuteAsync(controllerContext, cancellationToken);
                    }
                }
            }
            else if (controllerContext.Request.RequestUri.AbsolutePath == "/api/Login")
            {
                return base.ExecuteAsync(controllerContext, cancellationToken);
            }
            return Task.FromResult(Request.CreateResponse(HttpStatusCode.BadRequest));
        }

        [NonAction]
        protected bool HalfMembersAreStudents()
        {
            CloudTable table = TableClient.GetTableReference("Members");
            table.CreateIfNotExists();

            TableQuery<Member> query = new TableQuery<Member>()
                .Where(TableQuery.GenerateFilterConditionForBool("Confirmed", QueryComparisons.Equal, true))
                .Select(new List<string> { "StudentId" });
            List<string> results = table.ExecuteQuery(query).Select(q => q.StudentId).ToList();

            int students = results.Count(q => q != null && q != string.Empty);
            int nonStudents = results.Count(q => q == null || q == string.Empty);
            if (students > nonStudents)
            {
                return true;
            }
            return false;
        }
        
        [NonAction]
        protected Member GetMember(string id)
        {
            CloudTable table = TableClient.GetTableReference("Members");
            table.CreateIfNotExists();

            TableQuery<Member> query = new TableQuery<Member>()
                .Where(TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.Equal, id));
            return table.ExecuteQuery(query).FirstOrDefault();
        }


    }
}
