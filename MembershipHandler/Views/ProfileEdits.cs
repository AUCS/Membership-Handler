using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace MembershipHandler.Views
{
    public class ProfileEdits
    {
        [EmailAddress]
        public string Email { get; set; }

        public string StudentId { get; set; }
    }
}