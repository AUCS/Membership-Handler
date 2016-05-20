using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MembershipHandler.Emails.Templates
{
    public class ConfirmEmail
    {
        public const string Text =
            @"

Hi <name>!

You have specified your email address to be: <email>.

Please confirm this as your email address by clicking here: http://aucs.club/link.html?type=Confirm&id=<link>

Thanks,

The AUCS Team



Not <name>? Someone else has entered your email address - Don't worry - Ignore the link above and we'll remove you from our system in 48 hours.



            ";
    }
}