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



You indicated that your Student Id is: <studentid>


To confirm this please click here: http://aucs.club/link.html?type=StudentConfirm&id=<linkid>



If you didn't sign up for the AUCS and believe you have recieved this email by mistake you don't have to do anything, we will remove your address from our system in 48 hours.


Thanks,

The AUCS Team

            ";
    }
}