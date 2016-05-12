using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace MembershipHandler.Models
{
    public class NewMember : TableEntity
    {
        public NewMember()
        {
            PartitionKey = "Member";
            RowKey = Guid.NewGuid().ToString();
        }

        public string Name { get; set; }
        public string FacebookId { get; set; }

        public bool Newsletter { get; set; }
        public bool Premium { get; set; }
        public bool Lifetime { get; set; }
        public bool Committee { get; set; }


    }




    public class Member : TableEntity
    {
        public Member()
        {
            PartitionKey = DateTime.Now.Year.ToString();
            RowKey = Guid.NewGuid().ToString();

            EmailConfirmed = false;
            StudentConfirmed = false;
            IsCommittee = false;
        }

        public string AUCSID
        {
            get
            {
                return RowKey;
            }
        }

        public bool EmailConfirmed { get; set; }

        public bool StudentConfirmed { get; set; }

        public bool IsCommittee { get; set; }

        public string Email { get; set; }
        
        public string StudentId { get; set; }

        public string Name { get; set; }

        public DateTime RegistrationDate { get; set; }

        public string SlackId { get; set; }
        



        public string ConfirmStudentId { get; set; }

        public string ConfirmEmailId { get; set; }
    }
}