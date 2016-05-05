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

Welcome to the Adelaide University Cheese Society <name>!



The Adelaide Univsersity Cheese Society is a society based around a shared love of cheese, thanks for registering to be a part of it! We hold frequent cheese tastings, tours, classes and sales, which we encourage you to come along to!


The AUCS is a modern club which was designed to be easy to be a part of even with the busiest of schedules. We use Slack (https://slack.com/) as a means of communicating, and you will get an invite to join us there after confirming this email. Be sure to like us on facebook too! (https://www.facebook.com/groups/aucs.club/)


To confirm your email address and join us on slack: http://aucs.club/link.html?type=EmailConfirm&id=<emailid>



If you didn't sign up for the AUCS and believe you have recieved this email by mistake you don't have to do anything, we will remove your address from our system in 48 hours.


Thanks,

The AUCS Team

            ";
    }
}