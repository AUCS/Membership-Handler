﻿using MembershipHandler.Models;
using MembershipHandler.Views;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace MembershipHandler.Controllers
{
    public class CurrentController : FacebookLoginController
    {
        [HttpPost]
        public Current Post()
        {
            Current response = new Current();

            response.Name = CurrentUser.Name;            
            response.StudentId = CurrentUser.StudentId;
            response.EmailAddress = CurrentUser.EmailAddress;

            if (!CurrentUser.Confirmed)
            {
                response.MembershipType = "Pending";
            }
            else if (CurrentUser.Lifetime)
            {
                response.MembershipType = "Lifetime";
            }
            else if (CurrentUser.Premium)
            {
                response.MembershipType = "Premium";
            }
            else
            {
                response.MembershipType = "Standard";
            }

            if (CurrentUser.Committee)
            {
                CloudTable committeeTable = TableClient.GetTableReference("Committee");
                TableQuery<Committee> query = new TableQuery<Committee>()
                    .Where(TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.Equal, CurrentUser.RowKey));
                Committee committee = committeeTable.ExecuteQuery(query).FirstOrDefault();

                if (committee != null)
                {
                    response.CommitteeTitle = committee.Title;
                }
            }
            else
            {
                response.CommitteeTitle = null;
            }

            return response;
        }
    }
}