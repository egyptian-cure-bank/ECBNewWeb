using ECBNewWeb.CustomAuthentication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using ECBNewWeb.DataAccess;
using ECBNewWeb.Models;

namespace ECBNewWeb.CustomAuthentication
{
    public class CustomAuthorizeAttribute : AuthorizeAttribute
    {
        public string UserName { get; set; }
        public string AccessLevel { get; set; }
        public string Facility { get; set; }
        protected virtual CustomPrincipal CurrentUser
        {
            get { return HttpContext.Current.User as CustomPrincipal; }
        }
        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            var isAuthorized = base.AuthorizeCore(httpContext);
            if (!isAuthorized)
            {
                return false;
            }
            string CurrentActionMethod = HttpContext.Current.Request.RequestContext.RouteData.Values["action"].ToString();
            string CurrentController = HttpContext.Current.Request.RequestContext.RouteData.Values["controller"].ToString();
            string UserNames = string.Join("", httpContext.User.Identity.Name.ToString());
            AuthenticationEntities db = new AuthenticationEntities();
            var UserId = db.logins.Where(u => u.username == UserNames).Select(u => u.id).FirstOrDefault();
            var AllAccessLevels = (from acc in db.AccessLevels
                                   join g in db.Grants on acc.Id equals g.AccessLevel
                                   join f in db.ActionMethods on g.ActionMethodId equals f.Id
                                   where g.UserId == UserId && f.ActionMethodName == CurrentActionMethod
                                   && f.Controller == CurrentController
                                   select acc.AccessLevel1 + f.ActionMethodName + f.Controller).ToList();
            List<string> SplitedAccessLevels = AccessLevel.Split(',').ToList();
            //Compare
            foreach (string CurrentAccessLevel in AllAccessLevels)
            {
                foreach (string Splited in SplitedAccessLevels)
                {
                    if (Splited == CurrentAccessLevel)
                    {
                        return true;
                    }
                    else
                    {
                        continue;
                    }
                }
            }
            return false;
        }
        private string[] GetUserRights(string v)
        {
            throw new NotImplementedException();
        }

        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            base.HandleUnauthorizedRequest(filterContext);
            RedirectToRouteResult routeData = null;

            if (AccessLevel == null)
            {
                routeData = new RedirectToRouteResult
                    (new RouteValueDictionary
                    (new
                    {
                        controller = "Account",
                        action = "Login",
                    }
                    ));
            }
            else
            {
                routeData = new RedirectToRouteResult
                (new RouteValueDictionary
                 (new
                 {
                     controller = "Error",
                     action = "AccessDenied"
                 }
                 ));
            }

            filterContext.Result = routeData;
        }

    }
}