using ECBNewWeb.CustomAuthentication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ECBNewWeb.Filters
{
    public class AuthFilterAttribute : ActionFilterAttribute, IActionFilter
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var httpContext = context.HttpContext;
            if (!httpContext.User.Identity.IsAuthenticated)
            {
                if (httpContext.Request.Url != null)
                {
                    var redirectOnSuccess = httpContext.Request.Url.AbsolutePath;
                    var urlHelper = new UrlHelper(context.RequestContext);
                    var url = urlHelper.RouteUrl(redirectOnSuccess);
                    httpContext.Response.Redirect(url, true);
                }
            }
            base.OnActionExecuting(context);
        }
    }
}