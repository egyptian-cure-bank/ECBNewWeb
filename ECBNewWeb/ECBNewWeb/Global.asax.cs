using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using System.Web.Security;
using ECBNewWeb.CustomAuthentication;
using ECBNewWeb.Models;

namespace ECBNewWeb
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }
        protected void Application_AuthenticateRequest(Object sender, EventArgs e)
        {
            //HttpCookie authCookie = Request.Cookies["Cookie1"];
            //FormsAuthentication.SignOut();
        }
        protected void Application_PostAuthenticateRequest(Object sender, EventArgs e)
        {
            HttpCookie authCookie = Request.Cookies["Cookie3"];
            if (authCookie != null)
            {
                FormsAuthenticationTicket authTicket = FormsAuthentication.Decrypt(authCookie.Value);

                var serializeModel = JsonConvert.DeserializeObject<CustomSerializeModel>(authTicket.UserData);
                CustomPrincipal principal = new CustomPrincipal(authTicket.Name);

                principal.UserId = serializeModel.UserId;
                principal.UsrName = serializeModel.UserName;
                principal.FirstName = serializeModel.FirstName;
                principal.LastName = serializeModel.LastName;
                principal.Roles = serializeModel.RoleName;
                HttpContext.Current.User = principal;
            }

        }
    }
}
