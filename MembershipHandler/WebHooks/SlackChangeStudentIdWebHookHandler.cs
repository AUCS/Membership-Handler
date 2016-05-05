using Microsoft.AspNet.WebHooks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Threading.Tasks;
using System.Collections.Specialized;

namespace MembershipHandler.WebHooks
{
    public class SlackChangeStudentIdWebHookHandler : WebHookHandler
    {
        public SlackChangeStudentIdWebHookHandler()
        {
            Receiver = SlackWebHookReceiver.ReceiverName;
        }

        public override Task ExecuteAsync(string receiver, WebHookHandlerContext context)
        {
            NameValueCollection command = context.GetDataOrDefault<NameValueCollection>();

            //context.I



            throw new NotImplementedException();
        }
    }
}