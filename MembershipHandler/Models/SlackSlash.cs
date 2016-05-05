using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace MembershipHandler.Models
{
    public class SlackSlash
    {
        [Required]
        public string token { get; set; }

        [Required]
        public string team_id { get; set; }
        
        public string channel_id { get; set; }
        
        public string channel_name { get; set; }

        [Required]
        public string user_id { get; set; }

        [Required]
        public string user_name { get; set; }

        [Required]
        public string command { get; set; }
        
        public string text { get; set; }

        [Required]
        public string response_url { get; set; }
    }
}