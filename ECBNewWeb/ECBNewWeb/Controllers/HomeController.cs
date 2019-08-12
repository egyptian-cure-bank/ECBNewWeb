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
        List<MenuModel> MenuSource;
        AuthenticationEntities db = new AuthenticationEntities();
        public ActionResult Index(int UId)
        {
            //if (Convert.ToString(UId) == string.Empty)
            //{
            //    return new HttpStatusCodeResult(HttpStatusCode.InternalServerError);
            //}
            //ViewBag.CurrentUser = (CustomMembershipUser)Membership.GetUser(HttpContext.User.Identity.Name, false);
            //ViewBag.AllRoles = Roles.GetRolesForUser(HttpContext.User.Identity.Name).ToList();
            //Session["CurrentUser"] =  Membership.GetUser(HttpContext.User.Identity.Name, false);
            return View();
        }
        public JsonResult MenuBulider()
        {
            //if (Convert.ToString(UId) == string.Empty)
            //{
            //    return new HttpStatusCodeResult(HttpStatusCode.InternalServerError);
            //}
            //ViewBag.CurrentUser = (CustomMembershipUser)Membership.GetUser(HttpContext.User.Identity.Name, false);
            //ViewBag.AllRoles = Roles.GetRolesForUser(HttpContext.User.Identity.Name).ToList();
            //Session["CurrentUser"] =  Membership.GetUser(HttpContext.User.Identity.Name, false);
            List<MenuModel> MenuSource;
            MenuSource = (from m in db.Menus
                          select new MenuModel()
                          {
                              MenuId = m.MenuId,
                              ArabicName = m.ArabicName,
                              EnglishName = m.EnglishName,
                              Action = m.Action,
                              Controller = m.Controller,
                              CssClass = m.CssClass,
                              Url = m.Url,
                              ParentMenuId = m.ParentMenuId,
                              Description = m.Description,
                              Sorting = m.Sorting
                              }).ToList();

            Session["Menus"] = CreateMenus(0, MenuSource);
            return Json(Session["Menus"],JsonRequestBehavior.AllowGet);
        }
        public IEnumerable<MenuModel> CreateMenus (int parentId,List<MenuModel> Source)
        {
            var result = from m in Source
                   where m.ParentMenuId == parentId
                          select new MenuModel()
                          {
                              MenuId = m.MenuId,
                              ArabicName = m.ArabicName,
                              EnglishName = m.EnglishName,
                              Action = m.Action,
                              Controller = m.Controller,
                              CssClass = m.CssClass,
                              Url = m.Url,
                              ParentMenuId = m.ParentMenuId,
                              Description = m.Description,
                              Sorting = m.Sorting,
                              Children = CreateMenus(m.MenuId,Source).ToList()
                          };
            return result;
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