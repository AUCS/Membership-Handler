using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MembershipHandler.Models
{
    public class Email : TableEntity
    {
        public Email()
        {
            PartitionKey = "Email";

            Confirmed = false;
            ConfirmLink = Guid.NewGuid().ToString();
        }

        public string Address { get; set; }

        public bool Confirmed { get; set; }

        public string ConfirmLink { get; set; }
    }
}