using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MembershipHandler.Models
{
    public class Confirm : TableEntity
    {
        [Obsolete]
        public Confirm() { }

        public Confirm(string type)
        {
            PartitionKey = type;
            Code = Guid.NewGuid().ToString();
            Created = DateTime.UtcNow;
        }

        public DateTime Created { get; set; }

        public string Value { get; set; }

        public string Code { get; set; }
    }
}