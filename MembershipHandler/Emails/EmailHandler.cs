using MembershipHandler.Models;
using Microsoft.Azure;
using SendGrid;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Web;

namespace MembershipHandler.Emails
{
    public static class EmailHandler
    {
        public static void SendEmailConfirm(Member member, Confirm confirm)
        {
            SendGridMessage myMessage = new SendGridMessage();

            myMessage.AddTo(confirm.Value);
            myMessage.From = new MailAddress("membership@aucs.club", "Adelaide Uni Cheese Society");
            myMessage.Subject = "Welcome to the AUCS!";
            myMessage.Text = Templates.ConfirmEmail.Text;
            myMessage.Text = myMessage.Text.Replace("<name>", member.Name);
            myMessage.Text = myMessage.Text.Replace("<emailid>", confirm.Code);

            var transportWeb = new Web(CloudConfigurationManager.GetSetting("SendGridAPIKey"));
            transportWeb.DeliverAsync(myMessage);
        }

        public static void SendStudentConfirm(Member member, Confirm confirm)
        {
            SendGridMessage myMessage = new SendGridMessage();

            myMessage.AddTo(confirm.Value + "@student.adelaide.edu.au");
            myMessage.From = new MailAddress("membership@aucs.club", "Adelaide Uni Cheese Society");
            myMessage.Subject = "Confirm your Student Id";
            myMessage.Text = Templates.ConfirmStudent.Text;
            myMessage.Text = myMessage.Text.Replace("<name>", member.Name);
            myMessage.Text = myMessage.Text.Replace("<studentid>", confirm.Value);
            myMessage.Text = myMessage.Text.Replace("<linkid>", confirm.Code);

            var transportWeb = new Web(CloudConfigurationManager.GetSetting("SendGridAPIKey"));
            transportWeb.DeliverAsync(myMessage);
        }
    }
}