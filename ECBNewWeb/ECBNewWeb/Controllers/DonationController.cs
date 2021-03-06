﻿using System;
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
using System.Data.Entity.Validation;
using System.Net;

namespace ECBNewWeb.Controllers
{
    public class DonationController : Controller
    {
        private CustomMembershipUser UserInfo;
        [AuthFilter]
        //[CustomAuthorize(AccessLevel = "CreateAddDonationsDonation,FullControlAddDonationsDonation")]
        public ActionResult AddDonations()
        {
            if (User.Identity.IsAuthenticated)
            {
                UserInfo = (CustomMembershipUser)Membership.GetUser(HttpContext.User.Identity.Name, false);
                Session["CurrentUser"] = UserInfo.FirstName+" "+UserInfo.MiddleName+" "+UserInfo.LastName;
            }
            DonationData _DonationData = new DonationData();
            _DonationData.MySites = PopulateSites();
            _DonationData.MyReceipts = PopulateReceipts();
            _DonationData.MyCurrency = PopulateCurrency();
            _DonationData.MyPurposes = PopulatePurpose();
            _DonationData.MyPayments = PopulatePayment();
            _DonationData.MyKnowingMethods = PopulateKnowingMethod();
            _DonationData.BankInfoChecked = "false";
            return View(_DonationData);
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
                            "And marketingrectype.Active = 1 "+
                            "And BookResposibilities.DoneFlag = 0";
            using (SqlConnection Conn = new SqlConnection(ConfigurationManager.ConnectionStrings["ECBConnectionString"].ConnectionString))
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
        public JsonResult GetNextReceiptNoFromRecType(int RecTypeId)
        {
            UserInfo = (CustomMembershipUser)Membership.GetUser(HttpContext.User.Identity.Name, false);
            DataTable dt = new DataTable();
            string Cmd = "Select min(BookResp.HandleBookReceiptId)as HBookRecId, "+
                        "min(BookResp.NextReceiptNo) as NextReceiptNo, "+
                        "(Select Max(market.no) "+
                        "From market "+
                        "Where market.ResponsibilityId = min(BookResp.RespId))as LastSavedRecNo, " +
                        "Max(CanceledReceipts.ReceiptNo) as LastCanceledReceiptNo, "+
                        "Min(HandleBookReceipts.LastReceiptNo) as LastReceiptNo "+
                        "From HandleBookReceipts "+
                        "Inner Join BookTypes "+
                        "on dbo.BookTypes.BookTypeId = dbo.HandleBookReceipts.BookTypeId "+
                        "Inner Join marketingrectype "+
                        "On dbo.BookTypes.RecTypeId = dbo.marketingrectype.id "+
                        "Inner Join BookResposibilities BookResp "+
                        "on BookResp.HandleBookReceiptId = dbo.HandleBookReceipts.BookReceiptId "+
                        "Left Join CanceledReceipts "+
                        "On BookResp.RespId = dbo.CanceledReceipts.ResponsibilityId "+
                        "Where dbo.BookTypes.RecTypeId = @RecTypeId "+
                        "And marketingrectype.Active = 1 "+
                        "And BookResp.DoneFlag = 0 "+
                        "And BookResp.EmployeeId = @EmployeeId ";
            string JsonString = null;
            using (SqlConnection Conn = new SqlConnection(ConfigurationManager.ConnectionStrings["ECBConnectionString"].ConnectionString))
            {
                Conn.Open();
                using (SqlCommand Command = new SqlCommand(Cmd, Conn))
                {
                    Command.Parameters.AddWithValue("@RecTypeId", RecTypeId);
                    Command.Parameters.AddWithValue("@EmployeeId", UserInfo.EmployeeId);
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
            UserInfo = (CustomMembershipUser)Membership.GetUser(HttpContext.User.Identity.Name, false);
            List<SelectListItem> Items = new List<SelectListItem>();
            using (MarketEntities db = new MarketEntities())
            {
                DonationData MySite = (from S in db.marketingsites
                                             join U in db.UserSites
                                             on S.id equals U.SiteId
                                             where U.UserId == UserInfo.UserId
                                             && U.Active == 1
                                             select new DonationData() { SiteId = S.id, SiteName = S.sitename,MaxAssignDate = U.AssignDate }).OrderByDescending(order=>order.MaxAssignDate).First();
                SelectListItem DisabledItem = new SelectListItem()
                {
                    Text = "",
                    Value = "-1"
                };
                Items.Add(DisabledItem);
                SelectListItem selectList = new SelectListItem()
                {
                    Text = MySite.SiteName,
                    Value = MySite.SiteId.ToString()
                };
                Items.Add(selectList);
            }
            return Items;
        }
        private List<SelectListItem> PopulateReceipts()
        {
            List<SelectListItem> Items = new List<SelectListItem>();
            string Cmd = "Select marketingrectype.id, (marketingrectype.name+' - '+convert(nvarchar,min(BookTypes.BookNo)))as ReceiptType " +
                        "From HandleBookReceipts " +
                        "Inner Join BookTypes " +
                        "on dbo.BookTypes.BookTypeId = dbo.HandleBookReceipts.BookTypeId " +
                        "Inner Join marketingrectype " +
                        "On dbo.BookTypes.RecTypeId = dbo.marketingrectype.id " +
                        "Inner Join MarketingLicenses " +
                        "On dbo.BookTypes.LicenseId = dbo.MarketingLicenses.Id " +
                        "Inner Join BookResposibilities " +
                        "on dbo.BookResposibilities.HandleBookReceiptId = dbo.HandleBookReceipts.BookReceiptId " +
                        "Where dbo.BookResposibilities.DoneFlag = 0 " +
                        "And marketingrectype.Active = 1 " +
                        "And BookTypes.Active = 1 " +
                        "And dbo.BookResposibilities.EmployeeId = @EmployeeId " +
                        "Group by marketingrectype.name,marketingrectype.id";
            using (SqlConnection Conn = new SqlConnection(ConfigurationManager.ConnectionStrings["ECBConnectionString"].ConnectionString))
            {
                Conn.Open();
                using (SqlCommand Command = new SqlCommand(Cmd, Conn))
                {
                    Command.Parameters.AddWithValue("@EmployeeId", UserInfo.EmployeeId);
                    SqlDataReader Reader = Command.ExecuteReader();
                    SelectListItem DisabledItem = new SelectListItem()
                    {
                        Text = "",
                        Value = "-1"
                    };
                    Items.Add(DisabledItem);
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
                        Value = C.CurrencyName,
                        Selected = true
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
                SelectListItem DisabledItem = new SelectListItem()
                {
                    Text = "",
                    Value = "-1"
                };
                Items.Add(DisabledItem);
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
                                                select new DonationData() { PaymentId = S.Id, PaymentName = S.MethodName }).OrderBy(x => x.PaymentId).ToList<DonationData>();
                SelectListItem DisabledItem = new SelectListItem()
                {
                    Text = "",
                    Value = "-1"
                };
                Items.Add(DisabledItem);
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
        public JsonResult PopulateBankInfo()
        {
            List<SelectListItem> Items = new List<SelectListItem>();
            using (MarketEntities context = new MarketEntities())
            {
                List<DonationData> MyBanks = (from c in context.ChequeBanks
                                            select new DonationData() { ChequeBankId = c.ChequeBankId, ChequeBankName = c.ChequeBankName}).ToList<DonationData>();

                foreach (DonationData Banks in MyBanks)
                {

                    SelectListItem selectList = new SelectListItem()
                    {
                        Text = Banks.ChequeBankName,
                        Value = Banks.ChequeBankId.ToString()
                    };
                    Items.Add(selectList);
                }
            }
            DonationData banks = new DonationData()
            {
                MyChequeBanks = Items
            };
            return Json(banks.MyChequeBanks,JsonRequestBehavior.AllowGet);
        }
        public JsonResult AutoCompleteDonor(string prefix)
        {
            using (MarketEntities db = new MarketEntities())
            {
                var DonorName = db.doners.Where(x => x.name.Contains(prefix)).Select(x => new { x.name, x.id }).ToList();
                return Json(DonorName, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult SaveDonation(DonationData Donation)
        {
            UserInfo = (CustomMembershipUser)Membership.GetUser(HttpContext.User.Identity.Name, false);
            int RespId = 0;
            int NextReceiptNo = 0;
            int FirstReceiptNo = 0;
            int LastReceiptNo = 0;
            int RespCount = 0;
            int rowAffected = 0;
            int UpdateRecordCount = 0;
            try
            {
                if (Donation.BankInfoChecked == "false")
                {
                    ModelState.Remove("ChequeBankId");
                    ModelState.Remove("ChequeNumber");
                    ModelState.Remove("ChequeDate");
                }
                if (ModelState.IsValid)
                {
                    using (MarketEntities Market = new MarketEntities())
                    {
                        using (SqlConnection Conn = new SqlConnection(ConfigurationManager.ConnectionStrings["ECBConnectionString"].ConnectionString))
                        {
                            Conn.Open();
                            string Cmd = "Select min(BookResposibilities.RespId)as RespId " +
                                        "From HandleBookReceipts " +
                                        "Inner Join BookTypes " +
                                        "on dbo.BookTypes.BookTypeId = dbo.HandleBookReceipts.BookTypeId " +
                                        "Inner Join marketingrectype " +
                                        "On dbo.BookTypes.RecTypeId = dbo.marketingrectype.id " +
                                        "Inner Join MarketingLicenses " +
                                        "On dbo.BookTypes.LicenseId = dbo.MarketingLicenses.Id " +
                                        "Inner Join BookResposibilities " +
                                        "on dbo.BookResposibilities.HandleBookReceiptId = dbo.HandleBookReceipts.BookReceiptId " +
                                        "Where dbo.BookResposibilities.DoneFlag = 0 " +
                                        "And marketingrectype.Active = 1 " +
                                        "And dbo.BookResposibilities.EmployeeId = @EmployeeId " +
                                        "And dbo.marketingrectype.id = @RecTypeId ";
                            using (SqlCommand Command = new SqlCommand(Cmd, Conn))
                            {
                                Command.Parameters.AddWithValue("@EmployeeId", UserInfo.EmployeeId);
                                Command.Parameters.AddWithValue("@RecTypeId", Donation.RecId);
                                SqlDataReader Reader = Command.ExecuteReader();
                                if (Reader.Read())
                                {
                                    RespId = Reader.GetInt32(0);
                                }
                            }
                        }
                        if (RespId != 0)
                        {
                            using (SqlConnection Con = new SqlConnection(ConfigurationManager.ConnectionStrings["ECBConnectionString"].ConnectionString))
                            {
                                market DBDonation = new market();
                                ChequeInformation chequeBankInfo = new ChequeInformation();
                                //get the receipt name from selected receipt id
                                Donation.RecName = Market.marketingrectypes.Where(x => x.id == Donation.RecId).Select(y => y.name).FirstOrDefault();
                                DBDonation.dat = Donation.RecDate;
                                DBDonation.no = Convert.ToInt32(Donation.RecNumber);
                                DBDonation.name = Donation.DonorId;
                                DBDonation.amount = Donation.Amount;
                                DBDonation.currency = Donation.CurrencyName;
                                DBDonation.cash = Donation.PaymentId;
                                DBDonation.employee = UserInfo.EmployeeId;
                                DBDonation.type = Donation.RecId;
                                DBDonation.site = Donation.SiteId;
                                DBDonation.ResponsibilityId = RespId;
                                DBDonation.DonationPurposeId = Donation.PurpId;
                                DBDonation.combID = Donation.RecNumber.ToString() + Donation.RecName;
                                if (Donation.BankInfoChecked == "true")
                                {
                                    chequeBankInfo.ChequeBankId = Donation.ChequeBankId;
                                    chequeBankInfo.ChequeNo = Donation.ChequeNumber;
                                    chequeBankInfo.ChequeDate = Donation.ChequeDate;
                                    Market.ChequeInformations.Add(chequeBankInfo);
                                    Market.SaveChanges();
                                    DBDonation.ChequeInfoId = chequeBankInfo.Id;
                                }
                                // ModelState.AddModelError(string.Empty, "برجاء إستكمال بيانات الشيك");
                                Market.markets.Add(DBDonation);
                                //Market.SaveChanges();
                                rowAffected += Market.SaveChanges();
                                Con.Open();
                                string Cmd = "Select Count(market.ResponsibilityId) From market Where ResponsibilityId = @RespId ";
                                using (SqlCommand Command = new SqlCommand(Cmd, Con))
                                {
                                    Command.Parameters.AddWithValue("@RespId", RespId);
                                    RespCount = (Int32)Command.ExecuteScalar();
                                    if (RespCount > 0)
                                    {
                                        string Query = "Select IsNull(BookResposibilities.NextReceiptNo,0)as NextReceiptNo,IsNull(HandleBookReceipts.FirstReceiptNo,0) as FirstReceiptNo, " +
                                                        "HandleBookReceipts.LastReceiptNo " +
                                                        "From HandleBookReceipts " +
                                                        "Inner Join BookTypes " +
                                                        "on dbo.BookTypes.BookTypeId = dbo.HandleBookReceipts.BookTypeId " +
                                                        "Inner Join marketingrectype " +
                                                        "On dbo.BookTypes.RecTypeId = dbo.marketingrectype.id " +
                                                        "Inner Join MarketingLicenses " +
                                                        "On dbo.BookTypes.LicenseId = dbo.MarketingLicenses.Id " +
                                                        "Inner Join BookResposibilities " +
                                                        "on dbo.BookResposibilities.HandleBookReceiptId = dbo.HandleBookReceipts.BookReceiptId " +
                                                        "Where dbo.BookResposibilities.DoneFlag = 0 " +
                                                        "And marketingrectype.Active = 1 " +
                                                        "And BookResposibilities.RespId = @RespId ";
                                        using (SqlCommand Com = new SqlCommand(Query, Con))
                                        {
                                            Com.Parameters.AddWithValue("@RespId", RespId);
                                            SqlDataReader Reader = Com.ExecuteReader();
                                            if (Reader.Read())
                                            {
                                                NextReceiptNo = Reader.GetInt32(0);
                                                FirstReceiptNo = Reader.GetInt32(1);
                                                LastReceiptNo = Reader.GetInt32(2);
                                            }
                                            Reader.Close();
                                        }
                                        if (NextReceiptNo == 0)
                                        {
                                            string UpdateCommand = "Update BookResposibilities Set NextReceiptNo = @RecIncrement Where RespId = @RespId";
                                            using (SqlCommand UpdateCom = new SqlCommand(UpdateCommand, Con))
                                            {
                                                UpdateCom.Parameters.AddWithValue("@RespId", RespId);
                                                UpdateCom.Parameters.AddWithValue("@RecIncrement", FirstReceiptNo + 1);
                                                UpdateRecordCount = UpdateCom.ExecuteNonQuery();
                                            }
                                        }
                                        else if (NextReceiptNo > 0)
                                        {
                                            string UpdateCommand = "Update BookResposibilities Set NextReceiptNo = @RecIncrement Where RespId = @RespId";
                                            using (SqlCommand UpdateCom = new SqlCommand(UpdateCommand, Con))
                                            {
                                                UpdateCom.Parameters.AddWithValue("@RespId", RespId);
                                                UpdateCom.Parameters.AddWithValue("@RecIncrement", NextReceiptNo + 1);
                                                if (NextReceiptNo + 1 <= LastReceiptNo)
                                                {
                                                    UpdateRecordCount = UpdateCom.ExecuteNonQuery();
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        MarkBookResponsibilityAsDone(NextReceiptNo, LastReceiptNo, Donation.RecId, RespId, UserInfo.EmployeeId);
                        if (rowAffected > 0 && UpdateRecordCount > 0)
                        {
                            TempData["Msg"] = "تم الحفظ بنجاح";
                        }
                        else
                        {
                            TempData["Msg"] = "لم يتم الحفظ";
                        }
                        return RedirectToAction("AddDonations", Donation);
                    }
                }
                else
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors);
                    foreach (ModelState modelstate in ViewData.ModelState.Values)
                    {
                        foreach (ModelError error in modelstate.Errors)
                        {
                            TempData["ModelErrors"] += error.ErrorMessage;
                        }
                    }
                    return RedirectToAction("AddDonations", Donation);
                }
            }
            catch (DbEntityValidationException e)
            {
                foreach (var eve in e.EntityValidationErrors)
                {
                    Console.WriteLine("Entity of type \"{0}\" in state \"{1}\" has the following validation errors:",
                        eve.Entry.Entity.GetType().Name, eve.Entry.State);
                    foreach (var ve in eve.ValidationErrors)
                    {
                        Console.WriteLine("- Property: \"{0}\", Error: \"{1}\"",
                            ve.PropertyName, ve.ErrorMessage);
                    }
                }
                throw;
            }
        }

        public ActionResult CancelReceipt(string RecNumber, int? RecId)
        {
            DonationData model = new DonationData();
            model.RecNumber = RecNumber;
            model.RecId = RecId;
            return PartialView(model);
        }

        [HttpPost]
        public ActionResult CancelReceipt(DonationData Donation)
        {
            UserInfo = (CustomMembershipUser)Membership.GetUser(HttpContext.User.Identity.Name, false);
            int RespId = 0;
            int NextReceiptNo = 0;
            int FirstReceiptNo = 0;
            int LastReceiptNo = 0;
            int CanceledRecIdCount = 0;
            int rowAffected = 0;
            int UpdateRecordCount = 0;
            ModelState.Remove("ChequeBankId");
            ModelState.Remove("ChequeNumber");
            ModelState.Remove("ChequeDate");
            ModelState.Remove("recDate");
            if (!string.IsNullOrWhiteSpace(Donation.RecNumber))
            {
                using (MarketEntities Market = new MarketEntities())
                {
                    using (SqlConnection Conn = new SqlConnection(ConfigurationManager.ConnectionStrings["ECBConnectionString"].ConnectionString))
                    {
                        Conn.Open();
                        //get current responsibility according to userid and receipttypeid
                        string Cmd = "Select min(BookResposibilities.RespId)as RespId " +
                                    "From HandleBookReceipts " +
                                    "Inner Join BookTypes " +
                                    "on dbo.BookTypes.BookTypeId = dbo.HandleBookReceipts.BookTypeId " +
                                    "Inner Join marketingrectype " +
                                    "On dbo.BookTypes.RecTypeId = dbo.marketingrectype.id " +
                                    "Inner Join MarketingLicenses " +
                                    "On dbo.BookTypes.LicenseId = dbo.MarketingLicenses.Id " +
                                    "Inner Join BookResposibilities " +
                                    "on dbo.BookResposibilities.HandleBookReceiptId = dbo.HandleBookReceipts.BookReceiptId " +
                                    "Where dbo.BookResposibilities.DoneFlag = 0 " +
                                    "And marketingrectype.Active = 1 " +
                                    "And dbo.BookResposibilities.EmployeeId = @EmployeeId " +
                                    "And dbo.marketingrectype.id = @RecTypeId ";
                        using (SqlCommand Command = new SqlCommand(Cmd, Conn))
                        {
                            Command.Parameters.AddWithValue("@EmployeeId", UserInfo.EmployeeId);
                            Command.Parameters.AddWithValue("@RecTypeId", Donation.RecId);
                            SqlDataReader Reader = Command.ExecuteReader();
                            if (Reader.Read())
                            {
                                RespId = Reader.GetInt32(0);
                            }
                        }
                    }
                    if (RespId != 0)
                    {
                        CanceledReceipt DBCanceledReceipt = new CanceledReceipt();
                        DBCanceledReceipt.ReceiptNo = Convert.ToInt32(Donation.RecNumber);
                        DBCanceledReceipt.ResponsibilityId = RespId;
                        DBCanceledReceipt.Canceled = 1;
                        Market.CanceledReceipts.Add(DBCanceledReceipt);
                        rowAffected = Market.SaveChanges();
                        using (SqlConnection Con = new SqlConnection(ConfigurationManager.ConnectionStrings["ECBConnectionString"].ConnectionString))
                        {
                            Con.Open();
                            string Cmd = "Select count(CanceledReceiptId) From CanceledReceipts Where ResponsibilityId = @RespId ";
                            using (SqlCommand Command = new SqlCommand(Cmd, Con))
                            {
                                Command.Parameters.AddWithValue("@RespId", RespId);
                                CanceledRecIdCount = (Int32)Command.ExecuteScalar();
                                if (CanceledRecIdCount > 0)
                                {
                                    string Query = "Select IsNull(BookResposibilities.NextReceiptNo,0)as NextReceiptNo,IsNull(HandleBookReceipts.FirstReceiptNo,0) as FirstReceiptNo, " +
                                                    "HandleBookReceipts.LastReceiptNo " +
                                                    "From HandleBookReceipts " +
                                                    "Inner Join BookTypes " +
                                                    "on dbo.BookTypes.BookTypeId = dbo.HandleBookReceipts.BookTypeId " +
                                                    "Inner Join marketingrectype " +
                                                    "On dbo.BookTypes.RecTypeId = dbo.marketingrectype.id " +
                                                    "Inner Join MarketingLicenses " +
                                                    "On dbo.BookTypes.LicenseId = dbo.MarketingLicenses.Id " +
                                                    "Inner Join BookResposibilities " +
                                                    "on dbo.BookResposibilities.HandleBookReceiptId = dbo.HandleBookReceipts.BookReceiptId " +
                                                    "Where dbo.BookResposibilities.DoneFlag = 0 " +
                                                    "And marketingrectype.Active = 1 " +
                                                    "And BookResposibilities.RespId = @RespId ";
                                    using (SqlCommand Com = new SqlCommand(Query, Con))
                                    {
                                        Com.Parameters.AddWithValue("@RespId", RespId);
                                        SqlDataReader Reader = Com.ExecuteReader();
                                        if (Reader.Read())
                                        {
                                            NextReceiptNo = Reader.GetInt32(0);
                                            FirstReceiptNo = Reader.GetInt32(1);
                                            LastReceiptNo = Reader.GetInt32(2);
                                        }
                                        Reader.Close();
                                    }
                                    if (NextReceiptNo == 0)
                                    {
                                        string UpdateCommand = "Update BookResposibilities Set NextReceiptNo = @RecIncrement Where RespId = @RespId";
                                        using (SqlCommand UpdateCom = new SqlCommand(UpdateCommand, Con))
                                        {
                                            UpdateCom.Parameters.AddWithValue("@RespId", RespId);
                                            UpdateCom.Parameters.AddWithValue("@RecIncrement", FirstReceiptNo + 1);
                                            UpdateRecordCount += UpdateCom.ExecuteNonQuery();
                                        }
                                    }
                                    else if (NextReceiptNo > 0)
                                    {
                                        string UpdateCommand = "Update BookResposibilities Set NextReceiptNo = @RecIncrement Where RespId = @RespId";
                                        using (SqlCommand UpdateCom = new SqlCommand(UpdateCommand, Con))
                                        {
                                            UpdateCom.Parameters.AddWithValue("@RespId", RespId);
                                            UpdateCom.Parameters.AddWithValue("@RecIncrement", NextReceiptNo + 1);
                                            if (NextReceiptNo + 1 <= LastReceiptNo)
                                            {
                                                UpdateRecordCount += UpdateCom.ExecuteNonQuery();
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                MarkBookResponsibilityAsDone(NextReceiptNo, LastReceiptNo, Donation.RecId, RespId, UserInfo.EmployeeId);
                if (rowAffected >0 && UpdateRecordCount >0)
                {
                    TempData["Msg"] = "تم إلغاء الإيصال بنجاح";
                }
                else
                {
                    TempData["Msg"] = "لم يتم الحفظ";
                }
                return RedirectToAction("AddDonations", Donation);
            }
            else
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors);
                TempData["ModelErrors"] = "برجاء إختيار دفتر";
                return RedirectToAction("AddDonations", Donation);
            }

        }
        [HttpPost]
        public void MarkBookResponsibilityAsDone(int NextReceiptNo,int LastReceiptNo,int? RecTypeId, int RespId,int? EmployeeId)
        {
            int CountRec = 0;
            int MaxRecId = 0;
            int UpdatedRecordsCount = 0;
            int LastSavedRecNo = 0;
            //Check If there is a canceled receipts for responsibility id
            string Cmd = "Select Count(ReceiptNo)AS RecCount FROM CanceledReceipts Where ResponsibilityId = @RespId";
            using (SqlConnection Conn = new SqlConnection(ConfigurationManager.ConnectionStrings["ECBConnectionString"].ConnectionString))
            {
                Conn.Open();
                using (SqlCommand Com = new SqlCommand(Cmd, Conn))
                {
                    Com.Parameters.AddWithValue("@RespId",RespId);
                    CountRec = (Int32)Com.ExecuteScalar();
                    if (CountRec > 0)
                    {
                        //if exists select latest related receipt
                        Com.CommandText = "Select Max(ReceiptNo)As MaxRecId FROM CanceledReceipts Where ResponsibilityId = @RespId";
                        //Com.Parameters.AddWithValue("@RespId", RespId);
                        MaxRecId = (Int32)Com.ExecuteScalar();
                        if (MaxRecId == LastReceiptNo)
                        {
                            Com.CommandText = "Update BookResposibilities " +
                                                "Set DoneFlag = 1 " +
                                                "Where RespId = @RespId ";
                            //Com.Parameters.AddWithValue("@RespId", RespId);
                            UpdatedRecordsCount = Com.ExecuteNonQuery();
                        }
                    }
                }
                Cmd = "Select Isnull((Select Max(market.no) " +
                        "From market " +
                        "Where market.ResponsibilityId = min(BookResp.RespId)) ,0)as LastSavedRecNo " +
                        "From HandleBookReceipts " +
                        "Inner Join BookTypes " +
                        "on dbo.BookTypes.BookTypeId = dbo.HandleBookReceipts.BookTypeId " +
                        "Inner Join marketingrectype " +
                        "On dbo.BookTypes.RecTypeId = dbo.marketingrectype.id " +
                        "Inner Join BookResposibilities BookResp " +
                        "on BookResp.HandleBookReceiptId = dbo.HandleBookReceipts.BookReceiptId " +
                        "Left Join CanceledReceipts " +
                        "On BookResp.RespId = dbo.CanceledReceipts.ResponsibilityId " +
                        "Where dbo.BookTypes.RecTypeId = @RecTypeId " +
                        "And marketingrectype.Active = 1 " +
                        "And BookResp.DoneFlag = 0 " +
                        "And BookResp.EmployeeId = @EmployeeId ";
                using (SqlCommand ComV2 = new SqlCommand(Cmd,Conn))
                {
                    ComV2.Parameters.AddWithValue("@RecTypeId", RecTypeId);
                    ComV2.Parameters.AddWithValue("@EmployeeId", EmployeeId);
                    LastSavedRecNo = (Int32)ComV2.ExecuteScalar();
                    if (LastSavedRecNo == LastReceiptNo)
                    {
                        ComV2.CommandText = "Update BookResposibilities " +
                                            "Set DoneFlag = 1 " +
                                            "Where RespId = @RespId ";
                        ComV2.Parameters.AddWithValue("@RespId", RespId);
                        UpdatedRecordsCount = ComV2.ExecuteNonQuery();
                    }
                }
            }
        }
        public ActionResult AllDonations(DonationData DonationModel)
        {
            return View();
        }
        [HttpPost]
        public JsonResult AllDonations()
        {
            UserInfo = (CustomMembershipUser)Membership.GetUser(HttpContext.User.Identity.Name, false);
            var list = new List<DonationData>();
            var draw = Request.Form.GetValues("draw").FirstOrDefault();
            var start = Request.Form.GetValues("start").FirstOrDefault();
            var length = Request.Form.GetValues("length").FirstOrDefault();
            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            var sortColumn = Request.Form.GetValues("columns[" + Request.Form.GetValues("order[0][column]").FirstOrDefault()
                                        + "][name]").FirstOrDefault();
            var sortColumnDir = Request.Form.GetValues("order[0][dir]").FirstOrDefault();
            var RecNoSearch = Request.Form.GetValues("columns[1][search][value]").FirstOrDefault();
            int RecNo = RecNoSearch == string.Empty?0 :Convert.ToInt32(RecNoSearch);
            int skip = start != null ? Convert.ToInt16(start) : 0;
            int recordsTotal = 0;
            MarketEntities db = new MarketEntities();
            DateTime CurrentDate = DateTime.Now.Date;
            list = (from m in db.markets
                    join B in db.BookResposibilities on m.ResponsibilityId equals B.RespId
                    join E in db.Employees on B.EmployeeId equals E.EmployeeId
                    join ms in db.marketingsites on m.site equals ms.id
                    join mr in db.marketingrectypes on m.type equals mr.id
                    join d in db.doners on m.name equals d.id
                    join p in db.PaymentMethods on m.cash equals p.Id
                    join bt in db.BookTypes on mr.id equals bt.RecTypeId
                    join h in db.HandleBookReceipts on new { X1 = (int?)bt.BookTypeId, X2 = (int?)B.HandleBookReceiptId } equals new { X1 = (int?)h.BookTypeId, X2 = (int?)h.BookReceiptId }
                    join l in db.MarketingLicenses on bt.LicenseId equals l.Id
                    where h.Active == 1 && (CurrentDate >= l.FromDate && CurrentDate <= l.ToDate) && B.EmployeeId == UserInfo.EmployeeId
                    select new DonationData()
                    {
                        id=m.id,
                        no = m.no,
                        FullEmployeeName = E.FirstName +" " +E.MiddleName +" " +E.LastName ,
                        SiteName = ms.sitename ,
                        RecName = mr.name,
                        DonorName = d.name,
                        PaymentName = p.MethodName,
                        Amount = m.amount,
                        FinanceApproval = m.FinApprov
                    }).ToList<DonationData>();
            if (RecNo != 0)
            {
                list = list.Where(a => RecNo == a.no).ToList<DonationData>();
            }
            recordsTotal = list.Count();
            var data = list.Skip(skip).Take(pageSize).ToList<DonationData>();
            return Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult _EditDonation(int id)
        {
            ViewBag.MySites = PopulateSites();
            ViewBag.MyCurrency = PopulateCurrency();
            ViewBag.MyPurposes = PopulatePurpose();
            ViewBag.MyPayments = PopulatePayment();
            ViewBag.MyKnowingMethods = PopulateKnowingMethod();
            //ViewBag.BankInfoChecked = "false";
            MarketEntities db = new MarketEntities();
            DonationData MyRec = (from m in db.markets
                                  join d in db.doners on m.name equals d.id
                                  join mr in db.marketingrectypes on m.type equals mr.id
                                  where m.id == id
                                  select new DonationData {
                                      id = m.id,
                                      no =m.no,
                                      DonorName = d.name,
                                      DonorId = d.id,
                                      SiteId = m.site,
                                      RecName = mr.name,
                                      RecDate = m.dat,
                                      Amount = m.amount,
                                      CurrencyName = m.currency,
                                      PurpId = m.DonationPurposeId,
                                      PaymentId = m.cash
                                  }).FirstOrDefault();
            return PartialView(MyRec);
        }
        [HttpPost]
        public ActionResult _EditDonation(DonationData DonationModel)
        {
            if (DonationModel.id == 0)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            using (MarketEntities Market = new MarketEntities())
            {
                var DonationToUpdate = Market.markets.Find(DonationModel.id);
                if (DonationModel.PaymentId == 3)//Save Cheque Info
                {
                    DonationToUpdate.name = DonationModel.DonorId;
                    DonationToUpdate.site = DonationModel.SiteId;
                    DonationToUpdate.dat = DonationModel.RecDate;
                    DonationToUpdate.amount = DonationModel.Amount;
                    DonationToUpdate.currency = DonationModel.CurrencyName;
                    DonationToUpdate.DonationPurposeId = DonationModel.PurpId;
                    DonationToUpdate.cash = DonationModel.PaymentId;
                    //get the the necessary cheque information
                    var MarketsCheque = (from m in Market.markets
                                         where m.id == DonationModel.id
                                         select m).SingleOrDefault();
                    var ChequeInfoToUpdate = Market.ChequeInformations.Find(MarketsCheque.ChequeInfoId);
                    int ChequeAffectedRows = 0;
                    ChequeInformation chequeBankInfo = new ChequeInformation();
                    if (ChequeInfoToUpdate == null)
                    {
                        chequeBankInfo.ChequeBankId = DonationModel.ChequeBankId;
                        chequeBankInfo.ChequeNo = DonationModel.ChequeNumber;
                        chequeBankInfo.ChequeDate = DonationModel.ChequeDate;
                        Market.ChequeInformations.Add(chequeBankInfo);
                        ChequeAffectedRows = Market.SaveChanges();
                        DonationToUpdate.ChequeInfoId = chequeBankInfo.Id;
                    }
                    else
                    {
                        ChequeInfoToUpdate.ChequeBankId = DonationModel.ChequeBankId;
                        ChequeInfoToUpdate.ChequeNo = DonationModel.ChequeNumber;
                        ChequeInfoToUpdate.ChequeDate = DonationModel.ChequeDate;
                    }
                    int AffectedRows = Market.SaveChanges();
                    TempData["Msg"] = (AffectedRows + ChequeAffectedRows) > 0 ? "تم الحفظ بنجاح" : "لم يتم الحفظ";
                }
                else
                {
                    ModelState.Remove("ChequeBankId");
                    ModelState.Remove("ChequeNumber");
                    ModelState.Remove("ChequeDate");
                    DonationToUpdate.name = DonationModel.DonorId;
                    DonationToUpdate.site = DonationModel.SiteId;
                    DonationToUpdate.dat = DonationModel.RecDate;
                    DonationToUpdate.amount = DonationModel.Amount;
                    DonationToUpdate.currency = DonationModel.CurrencyName;
                    DonationToUpdate.DonationPurposeId = DonationModel.PurpId;
                    DonationToUpdate.cash = DonationModel.PaymentId;
                    var MarketsCheque = Market.ChequeInformations.Find(DonationToUpdate.ChequeInfoId);
                    DonationToUpdate.ChequeInfoId = 0;
                    TryUpdateModel(DonationToUpdate);
                    Market.ChequeInformations.Remove(MarketsCheque);
                    int AffectedRows = Market.SaveChanges();
                    TempData["Msg"] = AffectedRows > 0 ? "تم الحفظ بنجاح" : "لم يتم الحفظ";
                }
            }
            return RedirectToAction("AllDonations");
        }
        public JsonResult GetChequeBankInfo(int id)
        {
            MarketEntities db = new MarketEntities();
            DonationData MyChequeBankInfo = (from ci in db.ChequeInformations
                                             join m in db.markets on ci.Id equals m.ChequeInfoId
                                             where m.id == id
                                             select new DonationData {
                                                 ChequeBankId = ci.ChequeBankId,
                                                 ChequeNumber = ci.ChequeNo,
                                                 ChequeDate = ci.ChequeDate
                                             }).FirstOrDefault();
            return Json(MyChequeBankInfo, JsonRequestBehavior.AllowGet);
        }
    }
}
