using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MembershipHandler.Models
{
    public class StudentId : TableEntity
    {
        public StudentId()
        {
            PartitionKey = "StudentId";

            Confirmed = false;
            ConfirmLink = Guid.NewGuid().ToString();
        }

        public string Id { get; set; }

        public bool Confirmed { get; set; }

        public string ConfirmLink { get; set; }
    }
}