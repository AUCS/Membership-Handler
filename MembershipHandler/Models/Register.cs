using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace MembershipHandler.Models
{
    public class Register
    {
        [Required]
        public string Name { get; set; }
        [EmailAddress]
        public string Email { get; set; }

        public string StudentId { get; set; }
    }
}