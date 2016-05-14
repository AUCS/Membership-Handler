using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MembershipHandler.Models
{
    public class Committee : TableEntity
    {
        public Committee()
        {
            PartitionKey = "Committee";
            
            SuspectedOfNegligence = false;
            VotesMissed = 0;
        }

        public string Title { get; set; }        
                
        public bool SuspectedOfNegligence { get; set; }

        public int VotesMissed { get; set; }

    }
}