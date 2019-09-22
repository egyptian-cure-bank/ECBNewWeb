using ECBNewWeb.CustomAuthentication;
using ECBNewWeb.DataAccess;
using ECBNewWeb.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using System.Data.Entity;
using System.Web.Script.Serialization;
using System.Security.Cryptography;
using System.Text;

namespace ECBNewWeb.Controllers
{
    [AllowAnonymous]
    public class AccountController : Controller
    {
        private CustomMembershipUser UserInfo;
        [HttpGet]
        public ActionResult Login()
        {

            if (User.Identity.IsAuthenticated)
            {
                return LogOut();
            }
            //ViewBag.ReturnUrl = ReturnUrl;
            return View();
        }
        [HttpPost]
        public ActionResult Login(LoginViewModel loginView, string ReturnUrl = "")
        {
            if (ModelState.IsValid)
            {
                CustomSerializeModel userModel = null;
                if (Membership.ValidateUser(loginView.UserName, loginView.Password))
                {
                    var user = (CustomMembershipUser)Membership.GetUser(loginView.UserName, false);
                    if (user != null)
                    {
                        userModel = new CustomSerializeModel()
                        {
                            UserId = user.UserId,
                            UserName = user.UserName,
                            FirstName = user.FirstName,
                            LastName = user.LastName
                        };
                        string userData = JsonConvert.SerializeObject(userModel);
                        FormsAuthenticationTicket authTicket = new FormsAuthenticationTicket(1, loginView.UserName, DateTime.Now, DateTime.Now.AddMinutes(2), false, userData);
                        string enTicket = FormsAuthentication.Encrypt(authTicket);
                        HttpCookie faCookie = new HttpCookie("Cookie3", enTicket);
                        Response.Cookies.Add(faCookie);
                        //var serializer = new JavaScriptSerializer();
                    }
                    //if (!string.IsNullOrEmpty(Request.Form["ReturnUrl"]))
                    //{
                    //    return RedirectToAction(Request.Form["ReturnUrl"].Split('/')[2]);
                    //}
                    //else
                    //{
                        return RedirectToAction("Index", "Home", new { UId = userModel.UserId });
                    //}
                }
                else
                {
                    @ViewBag.ErrorMessage = "خطأ بإسم المستخدم أو كلمة السر";
                    return View(loginView);
                }
            }
            //ModelState.AddModelError("", "Something Wrong : Username or Password invalid ^_^ ");
            return View(loginView);
        }
        public ActionResult ChangePassword()
        {
            return PartialView("_ChangePassword");
        }
        [HttpPost]
        public ActionResult ChangePassword(ChangePasswordModel model)
        {
            bool Changed;
            if (User.Identity.IsAuthenticated)
            {
                UserInfo = (CustomMembershipUser)Membership.GetUser(HttpContext.User.Identity.Name, false);
            }
            if (ModelState.IsValid)
            {
                string username = UserInfo.UserName;
                using (MarketEntities dbMarket = new MarketEntities())
                {
                    Employee Emp;
                    using (AuthenticationEntities dbContext = new AuthenticationEntities())
                    {
                        var user = (from us in dbContext.logins
                                    where string.Compare(username, us.username, StringComparison.OrdinalIgnoreCase) == 0
                                    select us).FirstOrDefault();

                        if (user == null)
                        {
                            return null;
                        }
                        else
                        {
                            Emp = dbMarket.Employees.Where(x => x.EmployeeId == user.employee_id).FirstOrDefault();
                        }
                        CustomMembershipUser MemberChangePassword = new CustomMembershipUser(user, Emp);
                        Changed = MemberChangePassword.ChangePassword(model.OldPassword, model.ConfirmPassword);
                        TempData["Msg"] = Changed == true ? "تم الحفظ بنجاح" : "لم يتم الحفظ";
                    }
                }
            }
            else
            {
                TempData["Msg"] = "لم يتم الحفظ";
            }
            return RedirectToAction("LogOut");
        }

        public JsonResult ValidateOldPassword(string OldPassword)
        {
            if (User.Identity.IsAuthenticated)
            {
                UserInfo = (CustomMembershipUser)Membership.GetUser(HttpContext.User.Identity.Name, false);
            }
            SHA1 s = new SHA1CryptoServiceProvider();
            var bytes = Encoding.UTF8.GetBytes(OldPassword);
            var bytess = s.ComputeHash(bytes);
            var sos = Convert.ToBase64String(bytess);
            using (AuthenticationEntities dbContext = new AuthenticationEntities())
            {
                LoginViewModel user = (from us in dbContext.logins
                                       where string.Compare(UserInfo.UserName, us.username, StringComparison.OrdinalIgnoreCase) == 0
                                       && string.Compare(sos, us.password, StringComparison.OrdinalIgnoreCase) == 0
                                       && us.active == 1
                                       select new LoginViewModel { UserId = us.id, UserName = us.username, Password = us.password }).FirstOrDefault();
                if (user == null)
                {
                    return Json(false,JsonRequestBehavior.AllowGet);
                }
            }
            return Json(true, JsonRequestBehavior.AllowGet);
        }

        public ActionResult LogOut()
        {
            HttpCookie cookie = new HttpCookie("Cookie3", "");
            cookie.Expires = DateTime.Now.AddMinutes(-1);
            Response.Cookies.Add(cookie);

            FormsAuthentication.SignOut();
            return RedirectToAction("Login", "Account", null);
        }

    }
}