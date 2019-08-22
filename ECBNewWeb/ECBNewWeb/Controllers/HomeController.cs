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
        private CustomMembershipUser UserInfo;
        AuthenticationEntities db = new AuthenticationEntities();
        public ActionResult Index(int UId)
        {
            if (User.Identity.IsAuthenticated)
            {
                UserInfo = (CustomMembershipUser)Membership.GetUser(HttpContext.User.Identity.Name, false);
                Session["CurrentUser"] = Membership.GetUser(HttpContext.User.Identity.Name, false);
            }

            //if (Convert.ToString(UId) == string.Empty)
            //{
            //    return new HttpStatusCodeResult(HttpStatusCode.InternalServerError);
            //}
            //ViewBag.CurrentUser = (CustomMembershipUser)Membership.GetUser(HttpContext.User.Identity.Name, false);
            //ViewBag.AllRoles = Roles.GetRolesForUser(HttpContext.User.Identity.Name).ToList();
            //Session["CurrentUser"] =  Membership.GetUser(HttpContext.User.Identity.Name, false);
            if (User.Identity.IsAuthenticated)
            {
                UserInfo = (CustomMembershipUser)Membership.GetUser(HttpContext.User.Identity.Name, false);
                Session["CurrentUser"] = Membership.GetUser(HttpContext.User.Identity.Name, false);
            }
            return View();
        }
        public JsonResult MenuBulider()
        {
            if (User.Identity.IsAuthenticated)
            {
                UserInfo = (CustomMembershipUser)Membership.GetUser(HttpContext.User.Identity.Name, false);
                Session["CurrentUser"] = Membership.GetUser(HttpContext.User.Identity.Name, false);
            }
            List<MenuModel> MenuSource;
            MenuSource = (from m in db.Menus
                          join mr in db.MenuRoles on m.MenuId equals mr.MenuId
                          join ur in db.UserRoles on mr.RoleId equals ur.RoleID
                          where ur.UserID == UserInfo.UserId
                          select new MenuModel()
                          {
                              MenuId = m.MenuId,
                              ArabicName = m.ArabicName,
                              EnglishName = m.EnglishName,
                              Action = m.ActionName,
                              Controller = m.ControllerName,
                              CssClass = m.CssClass,
                              Url = m.Url,
                              ParentMenuId = m.ParentMenuId,
                              Description = m.Description,
                              Sorting = m.Sorting
                              }).Distinct().ToList();

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
    }
}