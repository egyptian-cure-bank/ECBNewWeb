using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ECBNewWeb.CustomAuthentication;
using System.Web.Security;
using ECBNewWeb.DataAccess;
using ECBNewWeb.Models;
using System.Net;

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
            _DonorData.DonorOFs = PopulateMotabare3List();
            _DonorData.Freqs = PopulatFreqList();
            _DonorData.TypeContacts = PopulateTypeContactList();
            return View(_DonorData);
        }

        public List<SelectListItem> PopulateGovernments()
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
        public List<SelectListItem> PopulateGenderListForEdit()
        {
            var GenderListItems = new List<SelectListItem>
            {
                new SelectListItem {Text="--إختار نوع--", Value = "", Disabled = true,Selected = true },
                new SelectListItem { Text = "ذكر", Value = "ذكر" },
                new SelectListItem { Text = "انثى", Value = "انثى" }
            };
            DonorData Gender = new DonorData();
            Gender.Gender = GenderListItems;
            return GenderListItems;
        }
        public List<SelectListItem> PopulateTypeContactList()
        {
            List<SelectListItem> Items = new List<SelectListItem>();
            using (MarketEntities db = new MarketEntities())
            {
                List<DonorData> TypeContact = (from typeContact in db.TypeContacts
                                          where typeContact.Active == 1
                                          select new DonorData() { ContactId = typeContact.Id, TypeContactName = typeContact.ContactTypeName }).ToList<DonorData>();
                SelectListItem DisabledItem = new SelectListItem()
                {
                    Text = "--إختار وسيلة إتصال--",
                    Value = "",
                    Disabled = true,
                    Selected = true
                };
                Items.Add(DisabledItem);
                foreach (DonorData ContactType in TypeContact)
                {
                    SelectListItem selectList = new SelectListItem()
                    {
                        Text = ContactType.TypeContactName,
                        Value = ContactType.TypeContactName
                    };
                    Items.Add(selectList);
                }
            }
            return Items;
        }
        public List<SelectListItem> PopulateTypeContactListForEdit()
        {
            var ContactListItems = new List<SelectListItem>
            {
                new SelectListItem {Text="--إختار وسيلة إتصال--", Value = "", Disabled = true,Selected = true },
                new SelectListItem { Text = "رسالة موبايل", Value = "رسالة موبايل" },
                new SelectListItem { Text = "بريد ألكتروني", Value = "بريد ألكتروني" },
                new SelectListItem { Text = "بريد عادي", Value = "بريد عادي" }
            };
            return ContactListItems;
        }
        public List<SelectListItem> PopulateMotabare3List()
        {
            List<SelectListItem> Items = new List<SelectListItem>();
            using (MarketEntities db = new MarketEntities())
            {
                List<DonorData> MyDonorOf = (from donorof in db.DonorOfs
                                             where donorof.Active == 1
                                              select new DonorData() { DonorOfId = donorof.Id, DonorOfName = donorof.Name }).ToList<DonorData>();
                SelectListItem DisabledItem = new SelectListItem()
                {
                    Text = "--إختار متبرع--",
                    Value = "",
                    Disabled = true,
                    Selected = true
                };
                Items.Add(DisabledItem);
                foreach (DonorData DonOf in MyDonorOf)
                {
                    SelectListItem selectList = new SelectListItem()
                    {
                        Text = DonOf.DonorOfName,
                        Value = DonOf.DonorOfName
                    };
                    Items.Add(selectList);
                }
            }
            return Items;
        }
        public List<SelectListItem> PopulatFreqList()
        {
            List<SelectListItem> Items = new List<SelectListItem>();
            using (MarketEntities db = new MarketEntities())
            {
                List<DonorData> MyFreq = (from freq in db.DonationFrequencies
                                          where freq.Active == 1
                                          select new DonorData() { FreqId = freq.Id, FreqName = freq.FrequencyName }).ToList<DonorData>();
                SelectListItem DisabledItem = new SelectListItem()
                {
                    Text = "--إختار تكرار التبرع--",
                    Value = "",
                    Disabled = true,
                    Selected = true
                };
                Items.Add(DisabledItem);
                foreach(DonorData Freq in MyFreq)
                {
                    SelectListItem selectList = new SelectListItem()
                    {
                        Text = Freq.FreqName,
                        Value = Freq.FreqName
                    };
                    Items.Add(selectList);
                }
            }
            return Items;
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
                    DBDonors.Typecontact = Donor.TypeContactName;
                    DBDonors.motabare3 = Donor.DonorOfName;
                    DBDonors.freq = Donor.FreqName;
                    DBDonors.address = Donor.Address;
                    DBDonors.email = Donor.Email;
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
        public ActionResult donor(DonorData donorModel)
        {
            return View();
        }
        [HttpPost]
        public JsonResult donor()
        {
            var list = new List<DonorData>();
            var draw = Request.Form.GetValues("draw").FirstOrDefault();
            var start = Request.Form.GetValues("start").FirstOrDefault();
            var length = Request.Form.GetValues("length").FirstOrDefault();
            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            var sortColumn = Request.Form.GetValues("columns[" + Request.Form.GetValues("order[0][column]").FirstOrDefault()
                                        + "][name]").FirstOrDefault();
            var sortColumnDir = Request.Form.GetValues("order[0][dir]").FirstOrDefault();
            var DonorNameSearch = Request.Form.GetValues("columns[1][search][value]").FirstOrDefault();
            var DonorTeleSearch = Request.Form.GetValues("columns[2][search][value]").FirstOrDefault();
            int skip = start != null ? Convert.ToInt16(start) : 0;
            int recordsTotal = 0;
            MarketEntities db = new MarketEntities();
            list = (from a in db.doners
                    join cen in db.centers on a.cent_id equals cen.center_id
                    join gov in db.governments on cen.government_id equals gov.government_id
                    select new DonorData()
                    {
                        Id = a.id,
                        DonorName = a.name,
                        Title = a.title,
                        Tele = a.mob,
                        CenterName = cen.center_name,
                        GovernmentName = gov.government_name,
                        Address = a.address
                    }).ToList<DonorData>();
            if (!string.IsNullOrEmpty(DonorNameSearch))
            {
                list = list.Where(a=> DonorNameSearch.Contains(a.DonorName)).ToList<DonorData>();
            }
            if (!string.IsNullOrEmpty(DonorTeleSearch))
            {
                list = list.Where(a => a.Tele != null && a.Tele.Contains(DonorTeleSearch)).ToList<DonorData>();
            }
            //if (!(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
            //{
            //    list = list.OrderBy(sortColumn + " " + sortColumnDir);
            //}
            recordsTotal = list.Count();
            var data = list.Skip(skip).Take(pageSize).ToList<DonorData>();
            return Json(new {draw = draw,recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data},JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        //[CustomAuthorize(AccessLevel ="EditEditDonor")]
        public ActionResult Edit(int id)
        {
            ViewBag.MyGovernments = PopulateGovernments();
            ViewBag.Gender = PopulateGenderListForEdit();
            ViewBag.TypeContact = PopulateTypeContactList();
            ViewBag.Motabre3 = PopulateMotabare3List();
            ViewBag.Freq = PopulatFreqList();

            DonorData FilteredDonor;
            Center CenterGovernId;
            using (MarketEntities db = new MarketEntities())
            {
                FilteredDonor = (from d in db.doners
                                 where d.id == id
                                 select new DonorData()
                                 {
                                     DonorName = d.name,
                                     Title = d.title,
                                     Tele = d.mob,
                                     CenterId = d.cent_id,
                                     GenderValue = d.sex,
                                     Job = d.job,
                                     TypeContactName = d.Typecontact,
                                     DonorOfName  = d.motabare3,
                                     FreqName = d.freq,
                                     Address = d.address,
                                     WorkPlace = d.workplace,
                                     Email = d.email,
                                     Notes = d.notes
                                 }).FirstOrDefault();

                CenterGovernId = (from c in db.centers
                                  where c.center_id == FilteredDonor.CenterId
                                  select new Center() { GovernId = c.government_id }).FirstOrDefault();
                FilteredDonor.GovernmentId = CenterGovernId.GovernId;
                //Repopulate Centers regarding the GovernmentId
                List<SelectListItem> Items = new List<SelectListItem>();
                using (MarketEntities context = new MarketEntities())
                {
                    List<Center> MyCenter = (from c in context.centers
                                             where c.government_id == CenterGovernId.GovernId
                                             select new Center() { CenterId = c.center_id, CenterName = c.center_name }).ToList<Center>();

                    foreach (Center Cen in MyCenter)
                    {

                        SelectListItem selectList = new SelectListItem()
                        {
                            Text = Cen.CenterName,
                            Value = Cen.CenterId.ToString()
                        };
                        Items.Add(selectList);
                    }
                }
                Center Cens = new Center()
                {
                    MyCenters = Items
                };
                FilteredDonor.MyCenters = Cens.MyCenters;

            }
            return PartialView(FilteredDonor);
        }
        [HttpPost]
        public ActionResult SaveEdit(DonorData DonorVData)
        {
            if (DonorVData.Id == 0)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            using (MarketEntities Market = new MarketEntities())
            {
                var DonorToUpdate = Market.doners.Find(DonorVData.Id);
                DonorToUpdate.name = DonorVData.DonorName;
                DonorToUpdate.title = DonorVData.Title;
                DonorToUpdate.mob = DonorVData.Tele;
                DonorToUpdate.cent_id = DonorVData.CenterId;
                DonorToUpdate.sex = DonorVData.GenderValue;
                DonorToUpdate.job = DonorVData.Job;
                DonorToUpdate.Typecontact = DonorVData.TypeContactName;
                DonorToUpdate.motabare3 = DonorVData.DonorOfName;
                DonorToUpdate.freq = DonorVData.FreqName;
                DonorToUpdate.address = DonorVData.Address;
                DonorToUpdate.workplace = DonorVData.WorkPlace;
                DonorToUpdate.email = DonorVData.Email;
                DonorToUpdate.notes = DonorVData.Notes;
                TryUpdateModel(DonorToUpdate);
                Market.SaveChanges();
            }
                return RedirectToAction("doner");
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