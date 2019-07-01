using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ECBNewWeb.DataAccess;
using ECBNewWeb.Models;
using System.Net;
using System.Data.SqlClient;
using System.Configuration;
using ECBNewWeb.CustomAuthentication;
using System.Web.Security;

namespace ECBNewWeb.Controllers
{
    public class UserSiteController : Controller
    {
        private CustomMembershipUser UserInfo;
        // GET: UserSite
        public ActionResult AddUserToSite()
        {
            if (User.Identity.IsAuthenticated)
            {
                UserInfo = (CustomMembershipUser)Membership.GetUser(HttpContext.User.Identity.Name, false);
                Session["CurrentUser"] = Membership.GetUser(HttpContext.User.Identity.Name, false);
            }
            UserSiteModel userModel = new UserSiteModel();
            userModel.myEmployee = emp();
            userModel.mySites = sites();
            
            return View(userModel);
        }

        [HttpPost]
        public ActionResult AddUserToSite(UserSiteModel usermodel)
        {
            UserSite Add = new UserSite();
            if(ModelState.IsValid)
            {
                using (MarketEntities db = new MarketEntities())
                {
                    Add.UserId = usermodel.UserId;
                    Add.SiteId = usermodel.SiteId;
                    Add.AssignDate = DateTime.Now;
                    Add.Active = 1;
                    db.UserSites.Add(Add);
                    int rowAffected =db.SaveChanges();
                    TempData["Msg"] = rowAffected > 0 ? "تم الحفظ بنجاح" : "لم يتم الحفظ";
                }
            }
            else
            {
                TempData["Msg"] = "لم يتم الحفظ";
            }
            return RedirectToAction("AddUserToSite");
        }



        public ActionResult AllUserSites()
        {
            MarketEntities db = new MarketEntities();
            List<UserSiteModel> list = (from u in db.UserSites
                                        join e in db.UserLogins on u.UserId equals e.id
                                        join s in db.marketingsites on u.SiteId equals s.id
                                        select new UserSiteModel()
                                        {
                                            Id = u.Id,
                                            UserId = u.UserId,
                                            Firstname = e.FirstName,
                                            Lastname = e.LastName,
                                            SiteId = u.SiteId,
                                            Sitename = s.sitename,
                                            AssignDate = u.AssignDate,
                                            Active = u.Active                               
                                        }).ToList<UserSiteModel>();
            return View(list);
        }

        public ActionResult EditUserSites(int id)
        {
            UserSiteModel model = new UserSiteModel();
            using (MarketEntities db = new MarketEntities())
            {
              model = (from u in db.UserSites
                       join e in db.UserLogins on u.UserId equals e.id
                       join s in db.marketingsites on u.SiteId equals s.id
                       where u.Id == id
                       select new UserSiteModel()
                       {
                           UserId = u.UserId,
                           Firstname = e.FirstName,
                           Lastname = e.LastName,
                           SiteId = u.SiteId,
                           Sitename = s.sitename,
                           Active = u.Active
                       }).FirstOrDefault<UserSiteModel>();
            }
            model.myEmployee = emp();
            model.mySites = sites();
            return PartialView(model);
        }

        [HttpPost]
        public ActionResult EditUserSites(UserSiteModel model)
        {
            if (model.Id == 0)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            using (MarketEntities Market = new MarketEntities())
            {
                if (ModelState.IsValid)
                {
                    var modelToUpdate = Market.UserSites.Find(model.Id);
                    int MaxId = Market.UserSites.Where(x => model.UserId == x.UserId).Select(i => i.Id).Max();
                    if (MaxId==model.Id)
                    {
                        modelToUpdate.Active = model.Active;
                        TryUpdateModel(modelToUpdate);
                        int rowAffected = Market.SaveChanges();
                        TempData["Msg"] = rowAffected > 0 ? "تم الحفظ بنجاح" : "لم يتم الحفظ";
                    }
                    else
                    {
                        TempData["Msg"] = "لا يمكن تعديل هذا الموظف";
                        var errors = ModelState.Values.SelectMany(v => v.Errors);
                    }
                }
                else
                {
                    TempData["Msg"] = "لم يتم الحفظ";
                }

            }
            return RedirectToAction("AllUserSites", TempData["Msg"]);
        }

        public ActionResult IsUserAvailble(int userid,int SiteId)
        {
            bool IsExists = true;
            int Result =0;
            DateTime CurrenctDate = DateTime.Now.Date;
            string Cmd = "Select IsNull(Max(Id),0) From UserSites Where UserId = @UserId And SiteId = @SiteId";
            using (SqlConnection Conn = new SqlConnection(ConfigurationManager.ConnectionStrings["ECBConnectionString"].ConnectionString))
            {
                Conn.Open();
                using (SqlCommand Com = new SqlCommand(Cmd,Conn))
                {
                    Com.Parameters.AddWithValue("@UserId", userid);
                    Com.Parameters.AddWithValue("@SiteId", SiteId);
                    Result = (Int32)Com.ExecuteScalar();
                    if (Result > 0)
                    {
                        IsExists = false;
                    }
                }
            }
            return Json(IsExists, JsonRequestBehavior.AllowGet);
        }

        public ActionResult IsSiteAvailble(int userid,int SiteId)
        {
            bool IsExists = true;
            int Result = 0;
            DateTime CurrenctDate = DateTime.Now.Date;
            string Cmd = "Select IsNull(Max(Id),0) From UserSites Where UserId = @UserId And SiteId = @SiteId";
            using (SqlConnection Conn = new SqlConnection(ConfigurationManager.ConnectionStrings["ECBConnectionString"].ConnectionString))
            {
                Conn.Open();
                using (SqlCommand Com = new SqlCommand(Cmd, Conn))
                {
                    Com.Parameters.AddWithValue("@UserId", userid);
                    Com.Parameters.AddWithValue("@SiteId", SiteId);
                    Result = (Int32)Com.ExecuteScalar();
                    if (Result > 0)
                    {
                        IsExists = false;
                    }
                }
            }
            return Json(IsExists, JsonRequestBehavior.AllowGet);
        }

        public List<SelectListItem> sites()
        {

            List<SelectListItem> Items = new List<SelectListItem>();
            using (MarketEntities db = new MarketEntities())
            {
                List<MarketSitesModel> Mysites = (from e in db.marketingsites
                                                   select new MarketSitesModel() { id = e.id , sitename = e.sitename  }).ToList<MarketSitesModel>();
                SelectListItem DisabledList = new SelectListItem()
                {
                    Text = " ",
                    Value = " -1",
                    Selected = true
                };
                Items.Add(DisabledList);

                foreach (MarketSitesModel sites in Mysites)
                {
                    SelectListItem selectList = new SelectListItem()
                    {
                        Text = sites.sitename,
                        Value = sites.id.ToString(),
                    };
                    Items.Add(selectList);
                }
            }
            return Items;

        }
 
        public List<SelectListItem> emp()
        {
            List<SelectListItem> Items = new List<SelectListItem>();
            using (MarketEntities db = new MarketEntities())
            {
                List<LoginViewModel> MyEmployee = (from e in db.UserLogins
                                                   join m in db.Employees
                                                   on e.employee_id equals m.EmployeeId
                                                   where e.active == 1 && m.Active == 1 && m.ParentEmployeeId == UserInfo.EmployeeId 
                                                   select new LoginViewModel() { UserId = e.id, FullName = m.FirstName + " "+m.MiddleName+" " + m.LastName }).ToList<LoginViewModel>();
                SelectListItem DisabledList = new SelectListItem()
                {
                    Text = " ",
                    Value = " -1",
                    Selected = true
                };
                Items.Add(DisabledList);
                foreach (LoginViewModel employee in MyEmployee)
                {
                    SelectListItem selectList = new SelectListItem()
                    {
                        Text = employee.FullName,
                        Value = employee.UserId.ToString(),
                    };
                    Items.Add(selectList);
                }
            }
            return Items;
        }

    }
}