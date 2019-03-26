using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ECBNewWeb.CustomAuthentication;
using System.Web.Security;
using ECBNewWeb.DataAccess;
using ECBNewWeb.Models;

namespace ECBNewWeb.Controllers
{
    public class DonorController : Controller
    {
        [CustomAuthorize(AccessLevel = "CreateAddDonersDonor,FullControlAddDonersDonor")]
        public ActionResult AddDoners()
        {
            ViewBag.Msg = null;
            DonorData _DonorData = new DonorData();
            _DonorData.MyGovernments = PopulateGovernments();
            return View("~/Views/Market/AddDoners.cshtml",_DonorData);
        }

        private List<SelectListItem> PopulateGovernments()
        {
            List<SelectListItem> Items = new List<SelectListItem>();
            using (MarketEntities db = new MarketEntities())
            {
                List<DonorData> MyGov = (from gov in db.governments
                                         select new DonorData() { GovernmentId = gov.government_id, GovernmentName = gov.government_name }).ToList<DonorData>();
                foreach (DonorData Gov in MyGov)
                {

                    SelectListItem selectList = new SelectListItem()
                    {
                        Text = Gov.GovernmentName,
                        Value = Gov.GovernmentId.ToString(),
                    };

                    Items.Add(selectList);

                }
            }
            return Items;
            
        }
        public JsonResult PopulateCenters(int GovId)
        {
            List<SelectListItem> Items = new List<SelectListItem>();
            using (MarketEntities context = new MarketEntities())
            {
                List<DonorData> MyCenter = (from c in context.centers
                                         where c.government_id == GovId
                                         select new DonorData() { CenterId = c.center_id, CenterName = c.center_name }).ToList<DonorData>();

                foreach (DonorData Cen in MyCenter)
                {
                    
                    SelectListItem selectList = new SelectListItem()
                    {
                        Text = Cen.CenterName,
                        Value = Cen.CenterId.ToString()
                    };
                    Items.Add(selectList);
                }
            }
            DonorData Cens = new DonorData()
            {
                MyCenters = Items
            };
            return Json(Cens.MyCenters, JsonRequestBehavior.AllowGet);
        }
        public JsonResult PopulateGenderList()
        {
            var GenderListItems = new List<SelectListItem>
            {
                new SelectListItem {Text="--إختار نوع--", Value = "", Disabled = true,Selected = true },
                new SelectListItem { Text = "ذكر", Value = "ذكر" },
                new SelectListItem { Text = "انثى", Value = "انثى" }
            };
            DonorData Gender = new DonorData();
            Gender.Gender = GenderListItems;
            return Json(Gender.Gender, JsonRequestBehavior.AllowGet);
        }
        public JsonResult PopulateTypeContactList()
        {
            var ContactListItems = new List<SelectListItem>
            {
                new SelectListItem {Text="--إختار وسيلة إتصال--", Value = "", Disabled = true,Selected = true },
                new SelectListItem { Text = "رسالة موبايل", Value = "رسالة موبايل" },
                new SelectListItem { Text = "بريد ألكتروني", Value = "بريد ألكتروني" },
                new SelectListItem { Text = "بريد عادي", Value = "بريد عادي" }
            };
            DonorData Contact = new DonorData();
            Contact.TypeContacts = ContactListItems;
            return Json(Contact.TypeContacts, JsonRequestBehavior.AllowGet);
        }
        public JsonResult PopulateMotabare3List()
        {
            var Motabre3ListItems = new List<SelectListItem>
            {
                new SelectListItem {Text="--إختار متبرع--", Value = "", Disabled = true,Selected = true },
                new SelectListItem { Text = "بنك الشفاء", Value = "بنك الشفاء" },
                new SelectListItem { Text = "بنك الطعام", Value = "بنك الطعام" },
                new SelectListItem { Text = "شفاء و طعام", Value = "شفاء و طعام" }
            };
            DonorData Contact = new DonorData();
            Contact.Motabre3 = Motabre3ListItems;
            return Json(Contact.Motabre3, JsonRequestBehavior.AllowGet);
        }
        public JsonResult PopulatFreqList()
        {
            var FreqListItems = new List<SelectListItem>
            {
                new SelectListItem {Text="--إختار تكرار التبرع--", Value = "", Disabled = true,Selected = true },
                new SelectListItem { Text = "شهرى", Value = "شهرى" },
                new SelectListItem { Text = "متقطع", Value = "متقطع" }
            };
            DonorData Contact = new DonorData();
            Contact.Freq = FreqListItems;
            return Json(Contact.Freq, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SaveDonor(DonorData Donor)
        {
            DonorData _DonorData = new DonorData();
            using (MarketEntities Market = new MarketEntities())
            {

                if (ModelState.IsValid)
                {
                    doner DBDonors = new doner();
                    DBDonors.name = Donor.DonorName;
                    DBDonors.title = Donor.Title;
                    DBDonors.mob = Donor.Tele;
                    DBDonors.cent_id = Donor.CenterId;
                    DBDonors.sex = Donor.GenderValue;
                    DBDonors.job = Donor.Job;
                    DBDonors.workplace = Donor.WorkPlace;
                    DBDonors.Typecontact = Donor.ContactValue;
                    DBDonors.motabare3 = Donor.Motabre3Value;
                    DBDonors.freq = Donor.FreqValue;
                    DBDonors.address = Donor.Address;
                    DBDonors.notes = Donor.Notes;
                    Market.doners.Add(DBDonors);
                    int rowAffected = Market.SaveChanges();
                    ViewBag.Msg = "تم الحفظ بنجاح";
                    TempData["Msg"] = "تم الحفظ بنجاح";

                }
                else
                {
                    TempData["Msg"] = "لم يتم الحفظ";
                    ViewBag.Msg = "لم يتم الحفظ";
                    ModelState.AddModelError("Error", "لم يتم الحفظ");
                    _DonorData.MyGovernments = PopulateGovernments();
                    return RedirectToAction("AddDoners", Donor);
                }
            }
            
            _DonorData.MyGovernments = PopulateGovernments();
            return RedirectToAction("AddDoners", Donor);
        }
        //public ActionResult Cancel(DonorData Donor)
        //{
        //    return RedirectToAction(Request.UrlReferrer.ToString());
        //}

        public ActionResult Doner()
        {
            MarketEntities db = new MarketEntities();
            List<DonorData> list = (from a in db.doners
                                    join cen in db.centers on a.cent_id equals cen.center_id
                                    join gov in db.governments on cen.government_id equals gov.government_id
                                    select new DonorData()
                                    { 
                                        Id =a.id,
                                        DonorName = a.name,
                                        Title = a.title,
                                        Tele = a.mob,
                                        CenterName = cen.center_name,
                                        GovernmentName = gov.government_name,
                                        Address = a.address
                                    }).Take(20).ToList<DonorData>();

            return View("~/Views/Market/donor.cshtml", list);
        }


        [HttpGet]
        public ActionResult Edit(int id)
        {
         
            if (Request.IsAjaxRequest())
            {
                //MarketEntities db = new MarketEntities();
                //var donor = db.doners.Find(id);
                return PartialView("~/Views/Market/Edit.cshtml");
            }
            else
            {
                return View("Index");
            }
        }




        [HttpGet]
        public ActionResult Delete(int id)
        {

            if (Request.IsAjaxRequest())
            {
                //MarketEntities db = new MarketEntities();
                //var donor = db.doners.Find(id);
                return PartialView("~/Views/Market/Delete.cshtml");
            }
            else
            {
                return View("Index");
            }
        }




    }
}