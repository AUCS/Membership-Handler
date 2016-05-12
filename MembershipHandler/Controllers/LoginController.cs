using Facebook;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace MembershipHandler.Controllers
{
    public class LoginController : ApiController
    {
        [HttpGet]
        public string Get(int id)
        {
            var accessToken = "your access token here";
            var client = new FacebookClient(accessToken);
            dynamic me = client.Get("me");
            string aboutMe = me.about;

            return aboutMe;
        }
    }
}
