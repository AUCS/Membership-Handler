﻿using MembershipHandler.Models;
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
    public class SlackInvite : ApiController
    {
        [HttpGet]
        public HttpResponseMessage Get(string guid)
        {
            RegisterController.RemoveOldUnconfirmedAccounts();


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
                return Request.CreateResponse(HttpStatusCode.BadRequest, " ");
            }
            if (results[0].EmailConfirmed)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, " ");
            }

            Member member = results[0];
            if (!member.EmailConfirmed)
            {
                member.EmailConfirmed = true;
                TableOperation tableOperation = TableOperation.Replace(member);
                table.Execute(tableOperation);
            }

            SendSlackInvitation(results[0]);

            return Request.CreateResponse(HttpStatusCode.OK, member.Email + " has been confirmed.");
        }

        private void SendSlackInvitation(Member member)
        {
            // using slack api (undocumented method documented here: https://github.com/ErikKalkoken/slackApiDoc)
            var client = new RestClient("https://slack.com/api/");
            // client.Authenticator = new HttpBasicAuthenticator(username, password);

            var request = new RestRequest("user.admin.invite", Method.POST);
            request.AddParameter("token", "value"); // Authentication token (Requires scope: ??)
            request.AddParameter("email", member.Email);
            
            if (member.Name.Contains(' '))
            {
                int nameSpace = member.Name.IndexOf(' ');
                string firstName = member.Name.Substring(0, nameSpace);
                string lastName = member.Name.Substring(nameSpace, member.Name.Length)
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
            var content = response.Content; // raw content as string

            // or automatically deserialize result
            // return content type is sniffed but can be explicitly set via RestClient.AddHandler();
            RestResponse<Person> response2 = client.Execute<Person>(request);
            var name = response2.Data.Name;
        }
    }
}