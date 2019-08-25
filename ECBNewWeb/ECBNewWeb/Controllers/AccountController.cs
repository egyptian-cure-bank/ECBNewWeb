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

namespace ECBNewWeb.Controllers
{
    [AllowAnonymous]
    public class AccountController : Controller
    {
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
                    //if (Url.IsLocalUrl(ReturnUrl))
                    //{
                    //    return Redirect(ReturnUrl);
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