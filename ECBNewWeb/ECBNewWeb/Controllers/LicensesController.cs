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
    public class LicensesController : Controller
    {
        // GET: Licenses
        [HttpGet]
        public ActionResult AddLicenses()
        {
            return View();
        }


        [HttpPost]
        public ActionResult AddLicenses(MarketingLicensesModel license)
        {

            using (MarketEntities Market = new MarketEntities())
            {
                if (ModelState.IsValid)
                {
                    MarketingLicens dblicense = new MarketingLicens();
                    dblicense.LicenseName = license.LicenseName;
                    dblicense.FromDate = license.FromDate;
                    dblicense.ToDate = license.ToDate;
                    dblicense.Active = license.Active == 1 ? 1 : 0;
                    dblicense.CreatedDate = DateTime.Now;
                    Market.MarketingLicenses.Add(dblicense);
                    int rowAffected = Market.SaveChanges();
                    TempData["Msg"] = "تم الحفظ بنجاح";
                }
                else
                {
                    TempData["Msg"] = "لم يتم الحفظ";
                }
            }
            return View();
        }




        public ActionResult AllLicenses()
        {
            MarketEntities db = new MarketEntities();
            List<MarketingLicensesModel> list = (from a in db.MarketingLicenses
                                                 select new MarketingLicensesModel()
                                                 {
                                                     Id = a.Id,
                                                     LicenseName = a.LicenseName,
                                                     FromDate = a.FromDate,
                                                     ToDate = a.ToDate,
                                                     Active = a.Active
                                                 }).ToList<MarketingLicensesModel>();
            return View(list);
        }


        public ActionResult EditLicenses(int id)
        {
            MarketingLicensesModel Marketlicense;
            using (MarketEntities db = new MarketEntities())
            {
                Marketlicense = (from d in db.MarketingLicenses
                                 where d.Id == id
                                 select new MarketingLicensesModel()
                                 {
                                     LicenseName = d.LicenseName,
                                     FromDate = d.FromDate,
                                     ToDate = d.ToDate,
                                     Active = d.Active,
                                 }).FirstOrDefault();

                return PartialView(Marketlicense);
            }
        }

        [HttpPost]
        public ActionResult EditLicenses(MarketingLicensesModel model)
        {
            if(model.Id == 0)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            using (MarketEntities Market = new MarketEntities())
            {
                var LicenseToUpdate = Market.MarketingLicenses.Find(model.Id);
                LicenseToUpdate.LicenseName = model.LicenseName;
                LicenseToUpdate.FromDate= model.FromDate;
                LicenseToUpdate.ToDate = model.ToDate;
                LicenseToUpdate.Active= model.Active ;
                Market.SaveChanges();
            }
            return RedirectToAction("AllLicenses");
        }
    }
}