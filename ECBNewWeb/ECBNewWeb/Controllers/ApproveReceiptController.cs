using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ECBNewWeb.Models;
using ECBNewWeb.DataAccess;


namespace ECBNewWeb.Controllers
{
    public class ApproveReceiptController : Controller
    {
        // GET: ApproveReceipt
        public ActionResult addApproveReceipt()
        {
            DonationData model = new DonationData();
            ViewBag.Sites = sites(); // fill ddl_sites
            ViewBag.Banks = banks(); // fill ddl_ banks
            return View(model);
        }
        [HttpPost]
        public ActionResult addApproveReceipt(string receipt_id , int payment_method , string BankName , DateTime ? bankDepositeDate , string  receiptvoucher , DateTime ?receiptvoucherDate)
        {
            /// test success 
            int rowAffected = 0;
           string[] marketid;
           marketid =  receipt_id.Split(',');
            MarketEntities db = new MarketEntities();
            
            if(ModelState.IsValid)
            {
                if(payment_method == 1 ) // ايداع نقدى
                {
                    for(int i = 0; i < marketid.Length; i++)
                    {
                        // insert into Approve Receipt
                        var approvereceipt = new ApproveReceipt()
                        {
                            approveDate = DateTime.Now,
                            marketId = int.Parse(marketid[i]),
                            depositType = 1
                        };
                        db.ApproveReceipts.Add(approvereceipt);
                        rowAffected = db.SaveChanges();
                        // insert into cashDeposit
                        var cashdeposit = new cashDeposit()
                        {
                            receiptVoucher = receiptvoucher,
                            Date = receiptvoucherDate,
                            ApproveReceiptFK = approvereceipt.approveReceiptId
                        };
                        db.cashDeposits.Add(cashdeposit);
                        rowAffected = db.SaveChanges();
                        // update market FinApprov
                        int market_id = int.Parse(marketid[i]);
                        market marketmodel = db.markets.Where(x => x.id == market_id).FirstOrDefault();
                        marketmodel.FinApprov = "1";
                        rowAffected = db.SaveChanges();
                        TempData["Msg"] = rowAffected > 0 ? "تم الحفظ بنجاح" : "لم يتم الحفظ";
                    }
                }
                else //ايداع بنكى 
                {
                    for (int i = 0; i < marketid.Length; i++)
                    {
                        // insert into Approve Receipt
                        var approvereceipt = new ApproveReceipt()
                        {
                            approveDate = DateTime.Now,
                            marketId = int.Parse(marketid[i]),
                            depositType = 2
                        };
                        db.ApproveReceipts.Add(approvereceipt);
                        rowAffected = db.SaveChanges();
                        // insert into BankDesposit
                        var bankdeposit = new bankDeposit()
                        {
                            BankName = BankName,
                            date = bankDepositeDate,
                            ApproveReceiptFK = approvereceipt.approveReceiptId
                        };

                        db.bankDeposits.Add(bankdeposit);
                        rowAffected = db.SaveChanges();
                        // update market FinApprov
                        int market_id = int.Parse(marketid[i]);
                        market marketmodel = db.markets.Where(x => x.id == market_id).FirstOrDefault();
                        marketmodel.FinApprov = "1";
                        rowAffected = db.SaveChanges();
                        TempData["Msg"] = rowAffected > 0 ? "تم الحفظ بنجاح" : "لم يتم الحفظ";
                    }
                }
            
            }
            else
            {
                TempData["Msg"] = "لم يتم الحفظ";
            }

           return RedirectToAction("addApproveReceipt");
        }
        public ActionResult SomeControllerAction(int Siteid)
        {
            //var jsonResult = Json(getReceiptsBySite(Siteid), JsonRequestBehavior.AllowGet);
            var jsonResult = getReceiptsBySite(Siteid);
            jsonResult.MaxJsonLength = int.MaxValue;
            return jsonResult;
        }
        public JsonResult getReceiptsBySite(int Siteid)
        {
            MarketEntities db = new MarketEntities();
            List<DonationData> data = (from m in db.markets
                                           //join b in db.BookResposibilities on m.ResponsibilityId equals b.RespId
                                           //join h in db.HandleBookReceipts on b.HandleBookReceiptId equals h.BookReceiptId
                                           //join t in db.BookTypes on h.BookTypeId equals t.BookTypeId
                                       join s in db.marketingsites on m.site equals s.id
                                       join ty in db.marketingrectypes on m.type equals ty.id
                                       join emp in db.UserLogins on m.employee equals emp.id
                                       where m.site == Siteid && (m.FinApprov == null || m.FinApprov == "0")
                                       select new DonationData()
                                       {
                                           id = m.id,
                                           SiteId = m.site,
                                           SiteName = s.sitename,
                                           RecId = m.type,
                                           RecName = ty.name,
                                           Amount = m.amount,
                                           FirstName = emp.FirstName,
                                           LastName = emp.LastName , 
                                           no = m.no,
                                        
                                     }).ToList<DonationData>();
            return Json(data, JsonRequestBehavior.AllowGet);
        }
        
 

        public List<SelectListItem> sites()
        {

            List<SelectListItem> Items = new List<SelectListItem>();
            using (MarketEntities db = new MarketEntities())
            {
                List<MarketSitesModel> Mysites = (from e in db.marketingsites
                                                  select new MarketSitesModel() { id = e.id, sitename = e.sitename }).ToList<MarketSitesModel>();
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

        public List<SelectListItem> banks()
        {

            List<SelectListItem> Items = new List<SelectListItem>();
            using (MarketEntities db = new MarketEntities())
            {
                List<BanksModel> MyBanks = (from e in db.Banks
                                                  select new BanksModel() { id = e.id, BankName = e.BankName }).ToList<BanksModel>();
                SelectListItem DisabledList = new SelectListItem()
                {
                    Text = " ",
                    Value = " -1",
                    Selected = true
                };
                Items.Add(DisabledList);

                foreach (BanksModel banks in MyBanks)
                {
                    SelectListItem selectList = new SelectListItem()
                    {
                        Text = banks.BankName,
                        Value = banks.id.ToString(),
                    };
                    Items.Add(selectList);
                }
            }
            return Items;

        }

    }
}