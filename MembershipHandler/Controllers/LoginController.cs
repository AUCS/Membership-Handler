using Facebook;
using MembershipHandler.Filters;
using MembershipHandler.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace MembershipHandler.Controllers
{
    public class LoginController : BaseController
    {
        private const string AUCSGroupId = "1549671362000387";

        [HttpGet]
        [AllowCrossSiteOrigin]
        public string Get(string id)
        {
            if (id == null || id == string.Empty)
            {
                return "invalid token";
            }

            FacebookClient fbClient = new FacebookClient(id);
            dynamic fbUser = fbClient.Get("me");

            bool isInGroup = false;
            dynamic fbGroup = fbClient.Get(AUCSGroupId + "/Members");
            foreach (dynamic groupMember in fbGroup.data)
            {
                if (groupMember.id == fbUser.id)
                {
                    isInGroup = true;
                }
            }


            NewMember user = GetMember(fbUser.id);
            
        }

        [NonAction]
        private  NewMember CreateNewUser()
        {
            return null;
        }
    }
}
