using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ECBNewWeb.DataAccess;
using ECBNewWeb.Models;
using System.Net;

namespace ECBNewWeb.Controllers
{
    public class UserSiteController : Controller
    {
        // GET: UserSite
        public ActionResult AddUserToSite()
        {
            UserSiteModel userModel = new UserSiteModel();
            userModel.myEmployee = emp();
            userModel.mySites = sites();
            
            return View(userModel);
        }

        [HttpPost]
        public ActionResult AddUserToSite(UserSiteModel usermodel)
        {
            UserSite update = new UserSite();
            if(ModelState.IsValid)
            {
                using (MarketEntities db = new MarketEntities())
                {
                    update.UserId = usermodel.UserId;
                    update.SiteId = usermodel.SiteId;
                    update.AssignDate = DateTime.Now;
                    db.UserSites.Add(update);
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
                                            AssignDate = u.AssignDate                                     
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
                           Sitename = s.sitename
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
                    modelToUpdate.SiteId = model.SiteId;
                    modelToUpdate.UserId = model.UserId;
                    modelToUpdate.AssignDate = DateTime.Now;
                    TryUpdateModel(modelToUpdate);
                    int rowAffected = Market.SaveChanges();
                    TempData["Msg"] = rowAffected > 0 ? "تم الحفظ بنجاح" : "لم يتم الحفظ";
                }
                else
                {
                    TempData["Msg"] = "لم يتم الحفظ";
                }

            }
            return RedirectToAction("AllUserSites", TempData["Msg"]);
        }

        public ActionResult IsUserAvailble(int userid)
        {
            using (MarketEntities db = new MarketEntities())
            {
                try
                {
                    var tag = db.UserSites.Single(m => m.UserId == userid);
                    return Json(false, JsonRequestBehavior.AllowGet);
                }
                catch (Exception)
                {
                    return Json(true, JsonRequestBehavior.AllowGet);
                }
            }
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
                                                       where e.department == 4
                                                   select new LoginViewModel() { UserId = e.id, FullName = e.FirstName + " " + e.LastName }).ToList<LoginViewModel>();
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