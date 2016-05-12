﻿using Microsoft.Azure;
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

namespace MembershipHandler.Controllers
{
    public class BaseController : ApiController
    {
        private CloudStorageAccount storageAccount;
        protected CloudTableClient tableClient;

        public override Task<HttpResponseMessage> ExecuteAsync(HttpControllerContext controllerContext, CancellationToken cancellationToken)
        {
            // Retrieve the storage account from the connection string.
            storageAccount = CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting("StorageConnectionString"));
            // Create the table client.
            tableClient = storageAccount.CreateCloudTableClient();

            return base.ExecuteAsync(controllerContext, cancellationToken);
        }
        
        [NonAction]
        protected bool HalfMembersAreStudents()
        {
            CloudTable table = tableClient.GetTableReference("Members");
            table.CreateIfNotExists();

            TableQuery<Member> query = new TableQuery<Member>()
                .Where(TableQuery.GenerateFilterConditionForBool("EmailConfirmed", QueryComparisons.Equal, true))
                .Select(new List<string> { "StudentConfirmed" });
            List<bool> results = table.ExecuteQuery(query).Select(q => q.StudentConfirmed).ToList();

            int students = results.Count(q => q);
            int nonStudents = results.Count(q => !q);
            if (students > nonStudents)
            {
                return true;
            }
            return false;
        }

        [NonAction]
        protected NewMember GetMember(string id)
        {
            CloudTable table = tableClient.GetTableReference("Members");
            table.CreateIfNotExists();

            TableQuery<NewMember> query = new TableQuery<NewMember>()
                .Where(TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.Equal, id));
            return table.ExecuteQuery(query).FirstOrDefault();
        }
    }
}
