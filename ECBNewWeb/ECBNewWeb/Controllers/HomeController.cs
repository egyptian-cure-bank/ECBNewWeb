using ECBNewWeb.CustomAuthentication;
using ECBNewWeb.DataAccess;
using ECBNewWeb.Models;
using ECBNewWeb.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace ECBNewWeb.Controllers
{
    [AuthFilter]
    public class HomeController : Controller
    {
        public ActionResult Index(int UId)
        {
            if (Convert.ToString(UId) == string.Empty)
            {
                return new HttpStatusCodeResult(HttpStatusCode.InternalServerError);
            }
            //ViewBag.CurrentUser = (CustomMembershipUser)Membership.GetUser(HttpContext.User.Identity.Name, false);
            //ViewBag.AllRoles = Roles.GetRolesForUser(HttpContext.User.Identity.Name).ToList();
            Session["CurrentUser"] =  Membership.GetUser(HttpContext.User.Identity.Name, false);
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        public ActionResult rules()
        {
            return View();
        }

        public ActionResult test()
        {
            return View();
        }





    }
}