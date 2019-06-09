using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ECBNewWeb.CustomAuthentication;
using ECBNewWeb.DataAccess;
using ECBNewWeb.Models;
using System.Data.Entity.Infrastructure;
using System.Web.Security;
using ECBNewWeb.Filters;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using Newtonsoft.Json;

namespace ECBNewWeb.Controllers
{
    public class DonationController : Controller
    {
        public int UserId;
        [AuthFilter]
        [CustomAuthorize(AccessLevel = "CreateAddDonationsDonation,FullControlAddDonationsDonation")]
        public ActionResult AddDonations()
        {
            UserId = ((CustomMembershipUser)Membership.GetUser(User.Identity.Name, false)).UserId;
            Session["CurrentUser"] = Membership.GetUser(HttpContext.User.Identity.Name, false);
            DonationData _DonationData = new DonationData();
            _DonationData.MySites = PopulateSites();
            _DonationData.MyReceipts = PopulateReceipts();
            _DonationData.MyCurrency = PopulateCurrency();
            _DonationData.MyPurposes = PopulatePurpose();
            _DonationData.MyPayments = PopulatePayment();
            _DonationData.MyKnowingMethods = PopulateKnowingMethod();
            return View("~/Views/Market/AddDonations.cshtml",_DonationData);
        }
        public JsonResult GetReceiptNoFromRecType(int RecTypeId)
        {
            DataTable dt = new DataTable();
            string JsonString = null;
            string Cmd = "Select Top 1(BookTypes.BookNo),HandleBookReceipts.FirstReceiptNo "+
                        "From HandleBookReceipts "+
                        "Inner Join BookTypes "+
                        "on dbo.BookTypes.BookTypeId = dbo.HandleBookReceipts.BookTypeId "+
                        "Inner Join marketingrectype "+
                        "On dbo.BookTypes.RecTypeId = dbo.marketingrectype.id "+
                        "Inner Join BookResposibilities "+
                        "on dbo.BookResposibilities.HandleBookReceiptId = dbo.HandleBookReceipts.BookReceiptId "+
                        "Where dbo.BookTypes.RecTypeId = @RecTypeId "+
                        "And dbo.BookResposibilities.DeliveryDate is null ";
            using (SqlConnection Conn = new SqlConnection(ConfigurationManager.ConnectionStrings["ECB_MarketingConnectionString"].ConnectionString))
            {
                Conn.Open();
                using (SqlCommand Command = new SqlCommand(Cmd,Conn))
                {
                    Command.Parameters.AddWithValue("@RecTypeId", RecTypeId);
                    SqlDataAdapter adapt = new SqlDataAdapter(Command);
                    adapt.Fill(dt);
                    JsonSerializerSettings SerSettings = new JsonSerializerSettings();
                    SerSettings.Culture = System.Globalization.CultureInfo.InstalledUICulture;
                    JsonString = JsonConvert.SerializeObject(dt,SerSettings);
                }
            }
            return Json(JsonString,JsonRequestBehavior.AllowGet);
        }
        public JsonResult GetNextReceiptNoFromRecType(int RecTypeId)
        {
            DataTable dt = new DataTable();
            string Cmd = "Select BookResposibilities.NextReceiptNo "+
                        "From HandleBookReceipts "+
                        "Inner Join BookTypes "+
                        "on dbo.BookTypes.BookTypeId = dbo.HandleBookReceipts.BookTypeId "+
                        "Inner Join marketingrectype "+
                        "On dbo.BookTypes.RecTypeId = dbo.marketingrectype.id "+
                        "Inner Join BookResposibilities "+
                        "on dbo.BookResposibilities.HandleBookReceiptId = dbo.HandleBookReceipts.BookReceiptId "+
                        "Where dbo.BookTypes.RecTypeId = @RecTypeId "+
                        "And dbo.BookResposibilities.DeliveryDate is null";
            string JsonString = null;
            using (SqlConnection Conn = new SqlConnection(ConfigurationManager.ConnectionStrings["ECB_MarketingConnectionString"].ConnectionString))
            {
                Conn.Open();
                using (SqlCommand Command = new SqlCommand(Cmd, Conn))
                {
                    Command.Parameters.AddWithValue("@RecTypeId", RecTypeId);
                    SqlDataAdapter adapt = new SqlDataAdapter(Command);
                    adapt.Fill(dt);
                    JsonSerializerSettings SerSettings = new JsonSerializerSettings();
                    SerSettings.Culture = System.Globalization.CultureInfo.InstalledUICulture;
                    JsonString = JsonConvert.SerializeObject(dt, SerSettings);
                }
            }
            return Json(JsonString, JsonRequestBehavior.AllowGet);
        }
        private List<SelectListItem> PopulateSites()
        {
            List<SelectListItem> Items = new List<SelectListItem>();
            using (MarketEntities db = new MarketEntities())
            {
                List<DonationData> MySite = (from S in db.marketingsites
                                         select new DonationData() { SiteId = S.id, SiteName = S.sitename }).ToList<DonationData>();
                foreach (DonationData S in MySite)
                {
                    SelectListItem selectList = new SelectListItem()
                    {
                        Text = S.SiteName,
                        Value = S.SiteId.ToString()
                    };
                    Items.Add(selectList);

                }
            }
            return Items;
        }
        private List<SelectListItem> PopulateReceipts()
        {
            List<SelectListItem> Items = new List<SelectListItem>();
            string Cmd = "Select marketingrectype.id,Concat(marketingrectype.name,' - ',min(BookTypes.BookNo))as ReceiptType "+
                        "From HandleBookReceipts "+
                        "Inner Join BookTypes "+
                        "on dbo.BookTypes.BookTypeId = dbo.HandleBookReceipts.BookTypeId "+
                        "Inner Join marketingrectype "+
                        "On dbo.BookTypes.RecTypeId = dbo.marketingrectype.id "+
                        "Inner Join MarketingLicenses "+
                        "On dbo.BookTypes.LicenseId = dbo.MarketingLicenses.Id "+
                        "Inner Join BookResposibilities "+
                        "on dbo.BookResposibilities.HandleBookReceiptId = dbo.HandleBookReceipts.BookReceiptId "+
                        "Where dbo.BookResposibilities.DeliveryDate is null "+
                        "And marketingrectype.Active = 1 "+
                        "And dbo.BookResposibilities.EmployeeId = @UserId " +
                        "Group by marketingrectype.name,marketingrectype.id";
            using (SqlConnection Conn = new SqlConnection(ConfigurationManager.ConnectionStrings["ECB_MarketingConnectionString"].ConnectionString))
            {
                Conn.Open();
                using (SqlCommand Command = new SqlCommand(Cmd,Conn))
                {
                    Command.Parameters.AddWithValue("@UserId", UserId);
                    SqlDataReader Reader = Command.ExecuteReader();
                    while (Reader.Read())
                    {
                        SelectListItem selectList = new SelectListItem()
                        {
                            Text = Reader.GetString(1),
                            Value = Reader.GetInt32(0).ToString()
                        };
                        Items.Add(selectList);
                    }
                }
            }
            return Items;
        }
        public List<SelectListItem> PopulateCurrency()
        {

            List<SelectListItem> Items = new List<SelectListItem>();
            using (MarketEntities db = new MarketEntities())
            {
                List<DonationData> MyCurr = (from S in db.Currencies
                                            select new DonationData() { CurrencyId = S.Id, CurrencyName = S.CurrencyName }).ToList<DonationData>();
                foreach (DonationData C in MyCurr)
                {
                    SelectListItem selectList = new SelectListItem()
                    {
                        Text = C.CurrencyName,
                        Value = C.CurrencyName
                    };
                    Items.Add(selectList);
                }
            }
            return Items;
        }
        public List<SelectListItem> PopulatePurpose()
        {

            List<SelectListItem> Items = new List<SelectListItem>();
            using (MarketEntities db = new MarketEntities())
            {
                List<DonationData> MyPurpose = (from S in db.DonationPurposes
                                             select new DonationData() { PurpId = S.Id, PurpName = S.Purpose }).ToList<DonationData>();
                foreach (DonationData P in MyPurpose)
                {
                    SelectListItem selectList = new SelectListItem()
                    {
                        Text = P.PurpName,
                        Value = P.PurpId.ToString()
                    };
                    Items.Add(selectList);
                }
            }
            return Items;
        }
        public List<SelectListItem> PopulatePayment()
        {

            List<SelectListItem> Items = new List<SelectListItem>();
            using (MarketEntities db = new MarketEntities())
            {
                List<DonationData> MyPayment = (from S in db.PaymentMethods
                                                select new DonationData() { PaymentId = S.Id, PaymentName = S.MethodName }).OrderBy(x=>x.PaymentId).ToList<DonationData>();
                foreach (DonationData P in MyPayment)
                {
                    SelectListItem selectList = new SelectListItem()
                    {
                        Text = P.PaymentName,
                        Value = P.PaymentId.ToString()
                    };
                    Items.Add(selectList);
                }
            }
            return Items;
        }
        public List<SelectListItem> PopulateKnowingMethod()
        {

            List<SelectListItem> Items = new List<SelectListItem>();
            using (MarketEntities db = new MarketEntities())
            {
                List<DonationData> MyMethods = (from S in db.KnowingMethods
                                                select new DonationData() { KnowingId = S.Id, KnowingMethodName = S.Name }).ToList<DonationData>();
                foreach (DonationData K in MyMethods)
                {
                    SelectListItem selectList = new SelectListItem()
                    {
                        Text = K.KnowingMethodName,
                        Value = K.KnowingId.ToString()
                    };
                    Items.Add(selectList);
                }
            }
            return Items;
        }
        public JsonResult AutoCompleteDonor(string prefix)
        {
            using (MarketEntities db = new MarketEntities())
            {
                var DonorName = db.doners.Where(x => x.name.Contains(prefix)).Select(x => new { x.name,x.id }).ToList();
                return Json(DonorName, JsonRequestBehavior.AllowGet);
            }
        }
        [HttpPost]
        public ActionResult SaveDonation(DonationData Donation)
        {
            using (MarketEntities Market = new MarketEntities())
            {
                if (ModelState.IsValid)
                {
                    
                    //BookResposibility GetRespId = new BookResposibility();
                    int RespId = Market.BookResposibilities.Where(y => y.EmployeeId == UserId).Select(x => x.RespId).FirstOrDefault();
                    if (RespId != 0)
                    {
                        market DBDonation = new market();
                        //get the receipt name from selected id
                        Donation.RecName = Market.marketingrectypes.Where(x => x.id == Donation.RecId).Select(y => y.name).FirstOrDefault();
                        DBDonation.dat = Donation.RecDate;
                        DBDonation.no = Donation.RecNumber;
                        DBDonation.name = Donation.DonorId;
                        DBDonation.amount = Donation.Amount;
                        DBDonation.currency = Donation.CurrencyName;
                        DBDonation.cash = Donation.PaymentId;
                        DBDonation.employee = UserId;
                        DBDonation.type = Donation.RecId;
                        DBDonation.site = Donation.SiteId;
                        DBDonation.ResponsibilityId = RespId;
                        DBDonation.DonationPurposeId = Donation.PurpId;
                        DBDonation.combID = Donation.RecNumber.ToString() + Donation.RecName;
                        Market.markets.Add(DBDonation);
                        Market.SaveChanges();
                        //BookResposibility NextRecNo = new BookResposibility();
                    }
                }
            }
            return RedirectToAction("AddDonations", Donation);
        }
    }
}