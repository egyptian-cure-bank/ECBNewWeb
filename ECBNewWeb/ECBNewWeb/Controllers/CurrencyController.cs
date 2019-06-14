using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ECBNewWeb.DataAccess;
using ECBNewWeb.Models;

namespace ECBNewWeb.Controllers
{
    public class CurrencyController : Controller
    {
        public ActionResult AddCurrency()
        {
            return View();
        }
        [HttpPost]
        public ActionResult AddCurrency(CurrencyModel Curr)
        {
            if (ModelState.IsValid)
            {
                using (MarketEntities Entity = new MarketEntities())
                {
                    Currency CurrSaved = new Currency();
                    CurrSaved.CurrencyName = Curr.CurrName;
                    Entity.Currencies.Add(CurrSaved);
                    Entity.SaveChanges();
                    ModelState.Clear();
                }
            }
            return View();
        }

        public ActionResult CurrenyConversion()
        {
            ConversionRateModel ConversionModel = new ConversionRateModel();
            ConversionModel.SourceMyCurrencies = PopulateCurrency();
            ConversionModel.TargetMyCurrencies = PopulateCurrency();
            return View(ConversionModel);
        }
        private List<SelectListItem> PopulateCurrency()
        {
            List<SelectListItem> Items = new List<SelectListItem>();
            using (MarketEntities db = new MarketEntities())
            {
                List<CurrencyModel> MyCurrency = (from c in db.Currencies
                                             select new CurrencyModel() { CurrencyId = c.Id, CurrName = c.CurrencyName}).ToList<CurrencyModel>();
                SelectListItem DisabledItem = new SelectListItem()
                {
                    Text = "--إختار عملة--",
                    Value = "",
                    Disabled = true,
                    Selected = true
                };
                Items.Add(DisabledItem);
                foreach (CurrencyModel C in MyCurrency)
                {
                    SelectListItem selectList = new SelectListItem()
                    {
                        Text = C.CurrName,
                        Value = C.CurrencyId.ToString()
                    };
                    Items.Add(selectList);

                }
            }
            return Items;
        }
        public JsonResult CheckCurrencyNameDuplication(string CurrName)
        {
            bool IsExists = true;
            using (MarketEntities db = new MarketEntities())
            {
                IsExists = db.Currencies.Where(x => x.CurrencyName.Contains(CurrName)).Any();
                if (IsExists)//If True this means Already Exists, then will switched to false because of the remote attribute requirments
                {
                    IsExists = false;
                }
                else
                {
                    IsExists = true;
                }
            }
            return Json(IsExists, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public ActionResult SaveConversionRates(ConversionRateModel CurrModel)
        {
            if (ModelState.IsValid)
            {
                using (MarketEntities db = new MarketEntities())
                {
                    CurrencyCovnersionRate ConvRateDB = new CurrencyCovnersionRate();
                    ConvRateDB.SourceCurrency = CurrModel.SourceCurrencyId;
                    ConvRateDB.TargetCurrency = CurrModel.TargetCurrencyId;
                    ConvRateDB.FromDate = CurrModel.FromDate.Date;
                    ConvRateDB.ToDate = CurrModel.ToDate.Date;
                    ConvRateDB.Rate = CurrModel.ConversionRate;
                    ConvRateDB.Active = 1;
                    db.CurrencyCovnersionRates.Add(ConvRateDB);
                    db.SaveChanges();
                    ViewBag.Msg = "تم الحفظ بنجاح";
                    TempData["Msg"] = "تم الحفظ بنجاح";
                    return RedirectToAction("CurrenyConversion", CurrModel);
                }
            }
            else
            {
                ViewBag.Msg = "لم يتم الحفظ";
                TempData["Msg"] = "لم يتم الحفظ";
                return RedirectToAction("CurrenyConversion", CurrModel);
            }
        }
    }
}