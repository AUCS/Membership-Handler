using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace MembershipHandler.Models
{
    public class Member : TableEntity
    {
        public Member()
        {
            PartitionKey = "Member";

            Name = null;
            Confirmed = false;
            Premium = false;
            Lifetime = false;
            Committee = false;
            StudentId = null;
            EmailAddress = null;
        }

        public string Name { get; set; }
        public bool Confirmed { get; set; }
        
        public bool Premium { get; set; }
        public bool Lifetime { get; set; }
        public bool Committee { get; set; }

        public string StudentId { get; set; }
        public string EmailAddress { get; set; }
    }
}