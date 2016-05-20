using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MembershipHandler.Emails.Templates
{
    public class ConfirmStudent
    {
        public const string Text =
            @"

Hi <name>!

You have specified your student id to be: <studentid>.

Please confirm this as your student id by clicking here: http://aucs.club/link.html?type=Confirm&id=<link>

Thanks,

The AUCS Team



Not <name>? Someone else has entered your student id - Don't worry - Ignore the link above and we'll remove you from our system in 48 hours.



            ";
    }
}