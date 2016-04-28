using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MembershipHandler.Models
{
    public class Member : TableEntity
    {
        public Member()
        {
            PartitionKey = DateTime.Now.Year.ToString();
            RowKey = Guid.NewGuid().ToString();
        }

        public bool EmailConfirmed { get; set; }

        public string Email { get; set; }

        public string Name { get; set; }
        
        public bool IsAdelaideUniStudent { get; set; }

        public string AdelaideUniStudentId { get; set; }

        public string ConfirmEmailId { get; set; }
    }
}