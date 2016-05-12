using Facebook;
using MembershipHandler.Filters;
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
        [AllowCrossSiteOrigin]
        public string Get(string id)
        {
            if (id == null || id == string.Empty)
            {
                return "invalid token";
            }

            FacebookClient client = new FacebookClient(id);
            dynamic me = client.Get("me");
            string name = me.name;
            string id = me.id;

            return me;
        }
    }
}
