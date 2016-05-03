﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MembershipHandler.Emails
{
    public class RegisterEmail
    {
        public const string Text =
            @"

Welcome to the Adelaide University Cheese Society <name>!


The Adelaide Univsersity Cheese Society is a society based around a shared love of cheese, thanks for registering to be a part of it! We hold frequent cheese tastings, tours, classes and sales, which we encourage you to come along to!


The AUCS is a modern club which was built around being easy to be a part of even with the busiest schedules. We use Slack (https://slack.com/) as a means of communicating so we recommend any new members join us with it, however it is not required and we will try our best to keep our facebook page updated with our events too! (https://www.facebook.com/groups/aucs.club/)


To finalise your membership please click on of the following:

To confirm your email address and join us on slack: http://aucsmembershiphandler.azurewebsites.net/api/SlackInvite/<emailid>

To confirm your email address only: http://aucsmembershiphandler.azurewebsites.net/api/EmailConfirm/<emailid>


If you didn't sign up for the AUCS and believe you have recieved this email by mistake you don't have to do anything, we will remove your address from our system in 48 hours.


Thanks,
The AUCS Team

            ";
    }
}