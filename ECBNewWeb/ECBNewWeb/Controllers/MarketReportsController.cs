using CrystalDecisions.CrystalReports.Engine;
using CrystalDecisions.Shared;
using ECB_DB_i;
using ECBNewWeb.CustomAuthentication;
using ECBNewWeb.DataAccess;
using ECBNewWeb.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace ECBNewWeb.Controllers
{
    public class MarketReportsController : Controller
    {
        private CustomMembershipUser UserInfo;
        int RecTypeId = 0;
        int SiteId = 0;
        DateTime FromDate;
        DateTime ToDate;
        int PurposeId;
        public ActionResult MarketPerformance()
        {
            if (User.Identity.IsAuthenticated)
            {
                UserInfo = (CustomMembershipUser)Membership.GetUser(HttpContext.User.Identity.Name, true);
                Session["CurrentUser"] = Session["CurrentUser"] = Membership.GetUser(HttpContext.User.Identity.Name, false);
            }
            return View();
        }
        [HttpPost]
        public ActionResult MarketPerformance(FormCollection form)
        {
            if (User.Identity.IsAuthenticated)
            {
                UserInfo = (CustomMembershipUser)Membership.GetUser(HttpContext.User.Identity.Name, false);
            }
            string Cmd = "Select * From "+
                        "( "+
                        "Select dat, no, name, Sum(amount)amount, currency, sum(cash) cash, employee, type, note, site, "+
                        "combID, Rates, GrandTotal, Purpose "+
                        "From "+
                        "( "+
                        "Select dat, no, name, (amount) as amount, currency, (cash) as Cash, employee, type + '-' + Convert(NVARCHAR, BookNo) as type, note, site, "+
                        "combID, Isnull(Rate, 1) as Rates, Sum((amount * Isnull(Rate, 1)) + (cash * Isnull(Rate, 1)))Over(Partition by dat) as GrandTotal, Purpose "+
                        "From "+
                        "( "+
                        "SELECT dat, no, cod, doners.name,case when cash = 1 then amount else 0 end as amount, "+
                        "(currency + (case when cash = 1 then ' فيزا' when cash = 2 then ' نقدي' when cash = 3 then ' شيك' end)) as currency, "+
                        "CASE WHEN cash = 2 or cash = 3 THEN amount else 0 END AS cash, "+
                        "concat(login.FirstName, ' ', login.LastName) AS employee, "+
                        "marketingrectype.name AS TYPE, "+
                        "note, "+
                        "marketingsites.siteName AS site, "+
                        "combID, TargetCurr.CurrencyName, TargetCurr.Id as CurrencyId, DonationPurpose.Purpose, BookTypes.BookNo "+ 
                        "FROM   market "+
                        "INNER JOIN login "+
                        "ON login.id = market.employee "+
                        "INNER JOIN marketingsites "+
                        "ON marketingsites.id = market.site "+
                        "INNER JOIN marketingrectype "+
                        "ON marketingrectype.id = market.type "+
                        "INNER JOIN doners "+
                        "ON doners.id = market.name "+
                        "Inner Join Currency TargetCurr "+
                        "On TargetCurr.CurrencyName = market.currency "+
                        "Inner Join DonationPurpose "+
                        "On market.DonationPurposeId = DonationPurpose.Id "+
                        "Inner Join BookResposibilities "+
                        "On market.ResponsibilityId = BookResposibilities.RespId "+
                        "Inner Join HandleBookReceipts "+
                        "On BookResposibilities.HandleBookReceiptId = HandleBookReceipts.BookReceiptId "+
                        "Inner Join BookTypes "+
                        "On dbo.BookTypes.BookTypeId = HandleBookReceipts.BookTypeId "+
                        "Where market.site = 1 "+
                        "And employee = @UserId " +
                        "And market.dat = @DateParam " +
                        ")T "+
                        "Left Join(Select CurrencyCovnersionRates.SourceCurrency, CurrencyCovnersionRates.TargetCurrency, Rate From CurrencyCovnersionRates "+
                        "Where getdate() Between CurrencyCovnersionRates.FromDate And CurrencyCovnersionRates.ToDate)ConvRates "+
                        "On ConvRates.SourceCurrency = T.CurrencyId) "+
                        "First "+
                        "Group By dat, no, name, currency, employee, type, note, site, combID, Rates, GrandTotal, Purpose "+
                        "Union "+
                        "Select CanceledReceipts.ActualDate, ReceiptNo as no,null as [name],0.00 amount,null currency,0.00 cash,login.FirstName + ' ' + login.LastName employee, "+
                        "marketingrectype.[name] + '-' + Convert(NVARCHAR, BookNo) type,null note,null site,null combID,0.00 as Rates,0.00 GrandTotal,null Purpose "+
                        "FROM CanceledReceipts "+
                        "Inner Join BookResposibilities "+
                        "On dbo.BookResposibilities.RespId = dbo.CanceledReceipts.ResponsibilityId "+
                        "Inner Join UserSites "+
                        "On dbo.BookResposibilities.EmployeeId = dbo.UserSites.UserId "+
                        "INNER JOIN login "+
                        "ON login.id = BookResposibilities.EmployeeId "+
                        "And UserSites.UserId = [login].id "+
                        "Inner Join HandleBookReceipts "+
                        "On BookResposibilities.HandleBookReceiptId = HandleBookReceipts.BookReceiptId "+ 
                        "Inner Join BookTypes "+
                        "On HandleBookReceipts.BookTypeId = BookTypes.BookTypeId "+ 
                        "Inner Join marketingrectype "+
                        "On marketingrectype.id = BookTypes.RecTypeId "+
                        "Where dbo.UserSites.SiteId = 1 "+
                        "And dbo.BookResposibilities.EmployeeId = @UserId " +
                        "And Convert(Date, dbo.CanceledReceipts.ActualDate) = @DateParam " +
                        ")TT "+
                        "Order by type, Cash, no";
            using (SqlConnection Conn = new SqlConnection(ConfigurationManager.ConnectionStrings["ECBConnectionString"].ConnectionString))
            {
                Conn.Open();
                using (SqlCommand Com = new SqlCommand(Cmd, Conn))
                {
                    Com.Parameters.AddWithValue("@UserId", UserInfo.UserId);
                    Com.Parameters.AddWithValue("@DateParam",Convert.ToDateTime(form["Date"].ToString()));
                    //List<Models.DonationData> DonationTable = new List<Models.DonationData>();
                    SqlDataAdapter Adapt = new SqlDataAdapter(Com);
                    DataTable dt = new DataTable();
                    Adapt.Fill(dt);
                    ReportDocument Doc = new ReportDocument();
                    Doc.Load(Path.Combine(Server.MapPath("~/CrystalReports/Marketing"),"rptMyMarketPerformance.rpt"));
                    Doc.SetDataSource(dt);
                    if (dt.Rows.Count > 0)
                    {
                        string Tafkeet = dt.Rows[0][12].ToString();
                        try
                        {
                            ToWord toWord = new ToWord(Convert.ToDecimal(Tafkeet), new CurrencyInfo(CurrencyInfo.Currencies.EGP));
                            Tafkeet = toWord.ConvertToArabic();
                            Doc.SetParameterValue("Tafqeet", Tafkeet);
                            Doc.SetParameterValue("Collector", UserInfo.FirstName + " " + UserInfo.LastName);
                            Stream stream = Doc.ExportToStream(ExportFormatType.PortableDocFormat);
                            stream.Seek(0, SeekOrigin.Begin);
                            return File(stream,"application/pdf");
                        }
                        catch (Exception ex)
                        {
                            Tafkeet = ex.ToString();
                        }
                    }
                }
            }
            return View();
        }
        public ActionResult MarketGroupBrief()
        {
            MarketGroupBriefModel _MarketBrief = new MarketGroupBriefModel();
            _MarketBrief.MyUserList = PopulateUsers();
            _MarketBrief.MySites = PopulateSites();
            _MarketBrief.MyDonationPurpose = PopulateDonationPurpose();
            return View(_MarketBrief);
        }
        public ActionResult RunMarketGroupBriefCash(MarketGroupBriefModel MarketModel)
        {
            UserInfo = (CustomMembershipUser)Membership.GetUser(HttpContext.User.Identity.Name, true);
            RecTypeId = MarketModel.RecTypeId;
            SiteId = MarketModel.SiteId;
            FromDate = MarketModel.FromDate.Date;
            ToDate = MarketModel.ToDate.Date;
            PurposeId = MarketModel.DonationPurposeId;

            string Cmd = "SELECT sum(amount) as amount,DonationPurpose.Purpose, marketingsites.siteName as site FROM market " +
                        "INNER JOIN doners ON doners.id = market.name " +
                        "INNER JOIN DonationPurpose ON DonationPurpose.Id = market.DonationPurposeId " +
                        "INNER JOIN BookResposibilities ON market.ResponsibilityId = BookResposibilities.RespId " +
                        "INNER JOIN HandleBookReceipts ON HandleBookReceipts.BookReceiptId = BookResposibilities.HandleBookReceiptId " +
                        "INNER JOIN BookTypes ON BookTypes.BookTypeId = HandleBookReceipts.BookTypeId " +
                        "INNER JOIN login ON login.id = BookResposibilities.EmployeeId " +
                        "INNER JOIN marketingsites ON marketingsites.id = market.site " +
                        "INNER JOIN marketingrectype ON marketingrectype.id = BookTypes.RecTypeId " +
                         WCondition(MarketModel.UserNameCheck, MarketModel.DateCheck, MarketModel.RecTypeCheck, MarketModel.SiteCheck, MarketModel.PurposeCheck) + " And market.cash = 2 " +
                        "group by marketingsites.siteName,DonationPurpose.Purpose";
            using (SqlConnection Conn = new SqlConnection(ConfigurationManager.ConnectionStrings["ECBConnectionString"].ConnectionString))
            {
                Conn.Open();
                using (SqlCommand Com = new SqlCommand(Cmd, Conn))
                {
                    SqlDataAdapter Adapt = new SqlDataAdapter(Com);
                    DataTable dt = new DataTable();
                    Adapt.Fill(dt);
                    ReportDocument Doc = new ReportDocument();
                    Doc.Load(Path.Combine(Server.MapPath("~/CrystalReports/Marketing"), "rptMarketingBrief.rpt"));
                    Doc.SetDataSource(dt);
                    Doc.SetParameterValue("HeaderParam", "جمع التبرعات - نقدي");
                    Doc.SetParameterValue("EmptyParam", "");
                    if (MarketModel.DateCheck)
                    {
                        Doc.SetParameterValue("FromDate", MarketModel.FromDate.Date);
                        Doc.SetParameterValue("ToDate", MarketModel.ToDate.Date);
                    }
                    else
                    {
                        string Cmd2 = "SELECT min(dat)AS fromDate, max(dat)AS toDate " +
                                        "FROM market " +
                                        "INNER JOIN doners " +
                                        "ON doners.id = market.name " +
                                        "INNER JOIN DonationPurpose " +
                                        "ON DonationPurpose.Id = market.DonationPurposeId " +
                                        "INNER JOIN BookResposibilities " +
                                        "ON market.ResponsibilityId = BookResposibilities.RespId " +
                                        "INNER JOIN HandleBookReceipts " +
                                        "ON HandleBookReceipts.BookReceiptId = BookResposibilities.HandleBookReceiptId " +
                                        "INNER JOIN BookTypes " +
                                        "ON BookTypes.BookTypeId = HandleBookReceipts.BookTypeId " +
                                        "INNER JOIN login " +
                                        "ON login.id = BookResposibilities.EmployeeId " +
                                        "INNER JOIN marketingsites " +
                                        "ON marketingsites.id = market.Site " +
                                        "INNER JOIN marketingrectype " +
                                        "ON marketingrectype.id = BookTypes.RecTypeId " +
                                        WCondition(MarketModel.UserNameCheck, MarketModel.DateCheck, MarketModel.RecTypeCheck, MarketModel.SiteCheck, MarketModel.PurposeCheck) +
                                        " And market.cash = 2";
                        using (SqlCommand Com2 = new SqlCommand(Cmd2, Conn))
                        {
                            using (SqlDataReader Reader = Com2.ExecuteReader())
                            {
                                if (Reader.Read())
                                {
                                    try
                                    {
                                        Doc.SetParameterValue("FromDate", Reader.GetDateTime(0).Date.ToString());
                                        Doc.SetParameterValue("ToDate", Reader.GetDateTime(1).Date.ToString());
                                    }
                                    catch (Exception)
                                    {

                                        Doc.SetParameterValue("EmptyParam", "لا يوجد بيانات");
                                        Doc.SetParameterValue("FromDate", DateTime.Now.Date);
                                        Doc.SetParameterValue("ToDate", DateTime.Now.Date);
                                    }
                                }
                            }
                        }
                    }
                    Stream stream = Doc.ExportToStream(ExportFormatType.PortableDocFormat);
                    stream.Seek(0, SeekOrigin.Begin);
                    return File(stream, "application/pdf");
                }
            }
        }
        public ActionResult RunMarketGroupBriefVisa(MarketGroupBriefModel MarketModel)
        {
            UserInfo = (CustomMembershipUser)Membership.GetUser(HttpContext.User.Identity.Name, true);
            RecTypeId = MarketModel.RecTypeId;
            SiteId = MarketModel.SiteId;
            FromDate = MarketModel.FromDate.Date;
            ToDate = MarketModel.ToDate.Date;
            PurposeId = MarketModel.DonationPurposeId;

            string Cmd = "SELECT sum(amount) as amount,DonationPurpose.Purpose, marketingsites.siteName as site FROM market " +
                        "INNER JOIN doners ON doners.id = market.name " +
                        "INNER JOIN DonationPurpose ON DonationPurpose.Id = market.DonationPurposeId " +
                        "INNER JOIN BookResposibilities ON market.ResponsibilityId = BookResposibilities.RespId " +
                        "INNER JOIN HandleBookReceipts ON HandleBookReceipts.BookReceiptId = BookResposibilities.HandleBookReceiptId " +
                        "INNER JOIN BookTypes ON BookTypes.BookTypeId = HandleBookReceipts.BookTypeId " +
                        "INNER JOIN login ON login.id = BookResposibilities.EmployeeId " +
                        "INNER JOIN marketingsites ON marketingsites.id = market.site " +
                        "INNER JOIN marketingrectype ON marketingrectype.id = BookTypes.RecTypeId " +
                         WCondition(MarketModel.UserNameCheck, MarketModel.DateCheck, MarketModel.RecTypeCheck, MarketModel.SiteCheck, MarketModel.PurposeCheck) + " And market.cash = 1 " +
                        "group by marketingsites.siteName,DonationPurpose.Purpose";
            using (SqlConnection Conn = new SqlConnection(ConfigurationManager.ConnectionStrings["ECBConnectionString"].ConnectionString))
            {
                Conn.Open();
                using (SqlCommand Com = new SqlCommand(Cmd, Conn))
                {
                    SqlDataAdapter Adapt = new SqlDataAdapter(Com);
                    DataTable dt = new DataTable();
                    Adapt.Fill(dt);
                    ReportDocument Doc = new ReportDocument();
                    Doc.Load(Path.Combine(Server.MapPath("~/CrystalReports/Marketing"), "rptMarketingBrief.rpt"));
                    Doc.SetDataSource(dt);
                    Doc.SetParameterValue("HeaderParam", "جمع التبرعات - فيزا");
                    Doc.SetParameterValue("EmptyParam", "");
                    if (MarketModel.DateCheck)
                    {
                        Doc.SetParameterValue("FromDate", MarketModel.FromDate.Date);
                        Doc.SetParameterValue("ToDate", MarketModel.ToDate.Date);
                    }
                    else
                    {
                        string Cmd2 = "SELECT min(dat)AS fromDate, max(dat)AS toDate " +
                                        "FROM market " +
                                        "INNER JOIN doners " +
                                        "ON doners.id = market.name " +
                                        "INNER JOIN DonationPurpose " +
                                        "ON DonationPurpose.Id = market.DonationPurposeId " +
                                        "INNER JOIN BookResposibilities " +
                                        "ON market.ResponsibilityId = BookResposibilities.RespId " +
                                        "INNER JOIN HandleBookReceipts " +
                                        "ON HandleBookReceipts.BookReceiptId = BookResposibilities.HandleBookReceiptId " +
                                        "INNER JOIN BookTypes " +
                                        "ON BookTypes.BookTypeId = HandleBookReceipts.BookTypeId " +
                                        "INNER JOIN login " +
                                        "ON login.id = BookResposibilities.EmployeeId " +
                                        "INNER JOIN marketingsites " +
                                        "ON marketingsites.id = market.Site " +
                                        "INNER JOIN marketingrectype " +
                                        "ON marketingrectype.id = BookTypes.RecTypeId " +
                                        WCondition(MarketModel.UserNameCheck, MarketModel.DateCheck, MarketModel.RecTypeCheck, MarketModel.SiteCheck, MarketModel.PurposeCheck) +
                                        " And market.cash = 1";
                        using (SqlCommand Com2 = new SqlCommand(Cmd2, Conn))
                        {
                            using (SqlDataReader Reader = Com2.ExecuteReader())
                            {
                                if (Reader.Read())
                                {
                                    try
                                    {
                                        Doc.SetParameterValue("FromDate", Reader.GetDateTime(0).Date.ToString());
                                        Doc.SetParameterValue("ToDate", Reader.GetDateTime(1).Date.ToString());
                                    }
                                    catch (Exception)
                                    {
                                        Doc.SetParameterValue("EmptyParam", "لا يوجد بيانات");
                                        Doc.SetParameterValue("FromDate", DateTime.Now.Date);
                                        Doc.SetParameterValue("ToDate", DateTime.Now.Date);
                                    }
                                }
                            }
                        }
                    }
                    Stream stream = Doc.ExportToStream(ExportFormatType.PortableDocFormat);
                    stream.Seek(0, SeekOrigin.Begin);
                    return File(stream, "application/pdf");
                }
            }
        }
        public ActionResult RunMarketGroupBriefCheque(MarketGroupBriefModel MarketModel)
        {

            {
                UserInfo = (CustomMembershipUser)Membership.GetUser(HttpContext.User.Identity.Name, true);
                RecTypeId = MarketModel.RecTypeId;
                SiteId = MarketModel.SiteId;
                FromDate = MarketModel.FromDate.Date;
                ToDate = MarketModel.ToDate.Date;
                PurposeId = MarketModel.DonationPurposeId;

                string Cmd = "SELECT sum(amount) as amount,DonationPurpose.Purpose, marketingsites.siteName as site FROM market " +
                            "INNER JOIN doners ON doners.id = market.name " +
                            "INNER JOIN DonationPurpose ON DonationPurpose.Id = market.DonationPurposeId " +
                            "INNER JOIN BookResposibilities ON market.ResponsibilityId = BookResposibilities.RespId " +
                            "INNER JOIN HandleBookReceipts ON HandleBookReceipts.BookReceiptId = BookResposibilities.HandleBookReceiptId " +
                            "INNER JOIN BookTypes ON BookTypes.BookTypeId = HandleBookReceipts.BookTypeId " +
                            "INNER JOIN login ON login.id = BookResposibilities.EmployeeId " +
                            "INNER JOIN marketingsites ON marketingsites.id = market.site " +
                            "INNER JOIN marketingrectype ON marketingrectype.id = BookTypes.RecTypeId " +
                             WCondition(MarketModel.UserNameCheck, MarketModel.DateCheck, MarketModel.RecTypeCheck, MarketModel.SiteCheck, MarketModel.PurposeCheck) + " And market.cash = 3 " +
                            "group by marketingsites.siteName,DonationPurpose.Purpose";
                using (SqlConnection Conn = new SqlConnection(ConfigurationManager.ConnectionStrings["ECBConnectionString"].ConnectionString))
                {
                    Conn.Open();
                    using (SqlCommand Com = new SqlCommand(Cmd, Conn))
                    {
                        SqlDataAdapter Adapt = new SqlDataAdapter(Com);
                        DataTable dt = new DataTable();
                        Adapt.Fill(dt);
                        ReportDocument Doc = new ReportDocument();
                        Doc.Load(Path.Combine(Server.MapPath("~/CrystalReports/Marketing"), "rptMarketingBrief.rpt"));
                        Doc.SetDataSource(dt);
                        Doc.SetParameterValue("HeaderParam", "جمع التبرعات - شيك");
                        Doc.SetParameterValue("EmptyParam", "");
                        if (MarketModel.DateCheck)
                        {
                            Doc.SetParameterValue("FromDate", MarketModel.FromDate.Date);
                            Doc.SetParameterValue("ToDate", MarketModel.ToDate.Date);
                        }
                        else
                        {
                            string Cmd2 = "SELECT min(dat)AS fromDate, max(dat)AS toDate " +
                                            "FROM market " +
                                            "INNER JOIN doners " +
                                            "ON doners.id = market.name " +
                                            "INNER JOIN DonationPurpose " +
                                            "ON DonationPurpose.Id = market.DonationPurposeId " +
                                            "INNER JOIN BookResposibilities " +
                                            "ON market.ResponsibilityId = BookResposibilities.RespId " +
                                            "INNER JOIN HandleBookReceipts " +
                                            "ON HandleBookReceipts.BookReceiptId = BookResposibilities.HandleBookReceiptId " +
                                            "INNER JOIN BookTypes " +
                                            "ON BookTypes.BookTypeId = HandleBookReceipts.BookTypeId " +
                                            "INNER JOIN login " +
                                            "ON login.id = BookResposibilities.EmployeeId " +
                                            "INNER JOIN marketingsites " +
                                            "ON marketingsites.id = market.Site " +
                                            "INNER JOIN marketingrectype " +
                                            "ON marketingrectype.id = BookTypes.RecTypeId " +
                                            WCondition(MarketModel.UserNameCheck, MarketModel.DateCheck, MarketModel.RecTypeCheck, MarketModel.SiteCheck, MarketModel.PurposeCheck) +
                                            " And market.cash = 3";
                            using (SqlCommand Com2 = new SqlCommand(Cmd2, Conn))
                            {
                                using (SqlDataReader Reader = Com2.ExecuteReader())
                                {
                                    if (Reader.Read())
                                    {
                                        try
                                        {
                                            Doc.SetParameterValue("FromDate", Reader.GetDateTime(0).Date.ToString());
                                            Doc.SetParameterValue("ToDate", Reader.GetDateTime(1).Date.ToString());
                                        }
                                        catch (Exception)
                                        {
                                            Doc.SetParameterValue("EmptyParam", "لا يوجد بيانات");
                                            Doc.SetParameterValue("FromDate", DateTime.Now.Date);
                                            Doc.SetParameterValue("ToDate", DateTime.Now.Date);
                                        }
                                    }
                                }
                            }
                        }
                        Stream stream = Doc.ExportToStream(ExportFormatType.PortableDocFormat);
                        stream.Seek(0, SeekOrigin.Begin);
                        return File(stream, "application/pdf");
                    }
                }
            }
        }
        public ActionResult RunMarketGroupAll(MarketGroupBriefModel MarketModel)
        {
            UserInfo = (CustomMembershipUser)Membership.GetUser(HttpContext.User.Identity.Name, true);
            RecTypeId = MarketModel.RecTypeId;
            SiteId = MarketModel.SiteId;
            FromDate = MarketModel.FromDate.Date;
            ToDate = MarketModel.ToDate.Date;
            PurposeId = MarketModel.DonationPurposeId;

            string Cmd = "SELECT sum(amount) as amount,DonationPurpose.Purpose, marketingsites.siteName as site FROM market " +
                        "INNER JOIN doners ON doners.id = market.name " +
                        "INNER JOIN DonationPurpose ON DonationPurpose.Id = market.DonationPurposeId " +
                        "INNER JOIN BookResposibilities ON market.ResponsibilityId = BookResposibilities.RespId " +
                        "INNER JOIN HandleBookReceipts ON HandleBookReceipts.BookReceiptId = BookResposibilities.HandleBookReceiptId " +
                        "INNER JOIN BookTypes ON BookTypes.BookTypeId = HandleBookReceipts.BookTypeId " +
                        "INNER JOIN login ON login.id = BookResposibilities.EmployeeId " +
                        "INNER JOIN marketingsites ON marketingsites.id = market.site " +
                        "INNER JOIN marketingrectype ON marketingrectype.id = BookTypes.RecTypeId " +
                         WCondition(MarketModel.UserNameCheck, MarketModel.DateCheck, MarketModel.RecTypeCheck, MarketModel.SiteCheck, MarketModel.PurposeCheck) +
                        "group by marketingsites.siteName,DonationPurpose.Purpose";
            using (SqlConnection Conn = new SqlConnection(ConfigurationManager.ConnectionStrings["ECBConnectionString"].ConnectionString))
            {
                Conn.Open();
                using (SqlCommand Com = new SqlCommand(Cmd, Conn))
                {
                    SqlDataAdapter Adapt = new SqlDataAdapter(Com);
                    DataTable dt = new DataTable();
                    Adapt.Fill(dt);
                    ReportDocument Doc = new ReportDocument();
                    Doc.Load(Path.Combine(Server.MapPath("~/CrystalReports/Marketing"), "rptMarketingBrief.rpt"));
                    Doc.SetDataSource(dt);
                    Doc.SetParameterValue("HeaderParam", "جمع التبرعات - إجماليات");
                    Doc.SetParameterValue("EmptyParam", "");
                    if (MarketModel.DateCheck)
                    {
                        Doc.SetParameterValue("FromDate", MarketModel.FromDate.Date);
                        Doc.SetParameterValue("ToDate", MarketModel.ToDate.Date);
                    }
                    else
                    {
                        string Cmd2 = "SELECT min(dat)AS fromDate, max(dat)AS toDate " +
                                        "FROM market " +
                                        "INNER JOIN doners " +
                                        "ON doners.id = market.name " +
                                        "INNER JOIN DonationPurpose " +
                                        "ON DonationPurpose.Id = market.DonationPurposeId " +
                                        "INNER JOIN BookResposibilities " +
                                        "ON market.ResponsibilityId = BookResposibilities.RespId " +
                                        "INNER JOIN HandleBookReceipts " +
                                        "ON HandleBookReceipts.BookReceiptId = BookResposibilities.HandleBookReceiptId " +
                                        "INNER JOIN BookTypes " +
                                        "ON BookTypes.BookTypeId = HandleBookReceipts.BookTypeId " +
                                        "INNER JOIN login " +
                                        "ON login.id = BookResposibilities.EmployeeId " +
                                        "INNER JOIN marketingsites " +
                                        "ON marketingsites.id = market.Site " +
                                        "INNER JOIN marketingrectype " +
                                        "ON marketingrectype.id = BookTypes.RecTypeId " +
                                        WCondition(MarketModel.UserNameCheck, MarketModel.DateCheck, MarketModel.RecTypeCheck, MarketModel.SiteCheck, MarketModel.PurposeCheck);
                        using (SqlCommand Com2 = new SqlCommand(Cmd2, Conn))
                        {
                            using (SqlDataReader Reader = Com2.ExecuteReader())
                            {
                                if (Reader.Read())
                                {
                                    try
                                    {
                                        Doc.SetParameterValue("FromDate", Reader.GetDateTime(0).Date.ToString());
                                        Doc.SetParameterValue("ToDate", Reader.GetDateTime(1).Date.ToString());
                                    }
                                    catch (Exception)
                                    {

                                        Doc.SetParameterValue("EmptyParam", "لا يوجد بيانات");
                                        Doc.SetParameterValue("FromDate", DateTime.Now.Date);
                                        Doc.SetParameterValue("ToDate", DateTime.Now.Date);
                                    }
                                }
                            }
                        }
                    }
                    Stream stream = Doc.ExportToStream(ExportFormatType.PortableDocFormat);
                    stream.Seek(0, SeekOrigin.Begin);
                    return File(stream, "application/pdf");
                }
            }
        }
        public ActionResult RunMarketAdminDetails(MarketGroupBriefModel MarketModel)
        {
            UserInfo = (CustomMembershipUser)Membership.GetUser(HttpContext.User.Identity.Name, true);
            RecTypeId = MarketModel.RecTypeId;
            SiteId = MarketModel.SiteId;
            FromDate = MarketModel.FromDate.Date;
            ToDate = MarketModel.ToDate.Date;
            PurposeId = MarketModel.DonationPurposeId;

            string Cmd = "Select * From " +
                        "( " +
                        "Select dat, no, name, Sum(amount)amount, currency, sum(cash) cash, employee, type, note, site, " +
                        "combID, Rates, Purpose " +
                        "From " +
                        "( " +
                        "Select dat, no, name, (amount) as amount, currency, (cash) as Cash, employee, type + '-' + Convert(NVARCHAR, BookNo) as type, note, site, " +
                        "combID, Isnull(Rate, 1) as Rates, Purpose " +
                        "From " +
                        "( " +
                        "SELECT dat, no, cod, doners.name,case when cash = 1 then amount else 0 end as amount, " +
                        "(currency + (case when cash = 1 then ' فيزا' when cash = 2 then ' نقدي' when cash = 3 then ' شيك' end)) as currency, " +
                        "CASE WHEN cash = 2 or cash = 3 THEN amount else 0 END AS cash, " +
                        "concat(login.FirstName, ' ', login.LastName) AS employee, " +
                        "marketingrectype.name AS TYPE, " +
                        "note, " +
                        "marketingsites.siteName AS site, " +
                        "combID, TargetCurr.CurrencyName, TargetCurr.Id as CurrencyId, DonationPurpose.Purpose, BookTypes.BookNo " +
                        "FROM   market " +
                        "INNER JOIN login " +
                        "ON login.id = market.employee " +
                        "INNER JOIN marketingsites " +
                        "ON marketingsites.id = market.site " +
                        "INNER JOIN marketingrectype " +
                        "ON marketingrectype.id = market.type " +
                        "INNER JOIN doners " +
                        "ON doners.id = market.name " +
                        "Inner Join Currency TargetCurr " +
                        "On TargetCurr.CurrencyName = market.currency " +
                        "Inner Join DonationPurpose " +
                        "On market.DonationPurposeId = DonationPurpose.Id " +
                        "Inner Join BookResposibilities " +
                        "On market.ResponsibilityId = BookResposibilities.RespId " +
                        "Inner Join HandleBookReceipts " +
                        "On BookResposibilities.HandleBookReceiptId = HandleBookReceipts.BookReceiptId " +
                        "Inner Join BookTypes " +
                        "On dbo.BookTypes.BookTypeId = HandleBookReceipts.BookTypeId " +
                        WCondition(MarketModel.UserNameCheck, MarketModel.DateCheck, MarketModel.RecTypeCheck, MarketModel.SiteCheck, MarketModel.PurposeCheck)+
                        " )T " +
                        "Left Join(Select CurrencyCovnersionRates.SourceCurrency, CurrencyCovnersionRates.TargetCurrency, Rate From CurrencyCovnersionRates " +
                        "Where getdate() Between CurrencyCovnersionRates.FromDate And CurrencyCovnersionRates.ToDate)ConvRates " +
                        "On ConvRates.SourceCurrency = T.CurrencyId) " +
                        "First " +
                        "Group By dat, no, name, currency, employee, type, note, site, combID, Rates, Purpose " +
                        "Union " +
                        "Select CanceledReceipts.ActualDate, ReceiptNo as no,null as [name],0.00 amount,null currency,0.00 cash,login.FirstName + ' ' + login.LastName employee, " +
                        "marketingrectype.[name] + '-' + Convert(NVARCHAR, BookNo) type,null note,null site,null combID,0.00 as Rates,null Purpose " +
                        "FROM CanceledReceipts " +
                        "Inner Join BookResposibilities " +
                        "On dbo.BookResposibilities.RespId = dbo.CanceledReceipts.ResponsibilityId " +
                        "Inner Join UserSites " +
                        "On dbo.BookResposibilities.EmployeeId = dbo.UserSites.UserId " +
                        "INNER JOIN login " +
                        "ON login.id = BookResposibilities.EmployeeId " +
                        "And UserSites.UserId = [login].id " +
                        "Inner Join HandleBookReceipts " +
                        "On BookResposibilities.HandleBookReceiptId = HandleBookReceipts.BookReceiptId " +
                        "Inner Join BookTypes " +
                        "On HandleBookReceipts.BookTypeId = BookTypes.BookTypeId " +
                        "Inner Join marketingrectype " +
                        "On marketingrectype.id = BookTypes.RecTypeId " +
                        CanceledWCondition(MarketModel.UserNameCheck, MarketModel.DateCheck, MarketModel.RecTypeCheck, MarketModel.SiteCheck) +
                        " )TT " +
                        "Order by type, Cash, no";
            using (SqlConnection Conn = new SqlConnection(ConfigurationManager.ConnectionStrings["ECBConnectionString"].ConnectionString))
            {
                Conn.Open();
                using (SqlCommand Com = new SqlCommand(Cmd, Conn))
                {
                    SqlDataAdapter Adapt = new SqlDataAdapter(Com);
                    DataTable dt = new DataTable();
                    Adapt.Fill(dt);
                    ReportDocument Doc = new ReportDocument();
                    Doc.Load(Path.Combine(Server.MapPath("~/CrystalReports/Marketing"), "rptMarketingAdminDetails.rpt"));
                    Doc.SetDataSource(dt);
                    if (dt.Rows.Count > 0)
                    {
                        Doc.SetParameterValue("CollectorName", UserInfo.FirstName + " " + UserInfo.LastName);
                        //string Tafkeet = dt.Rows[0][12].ToString();
                        //try
                        //{
                        //    ToWord toWord = new ToWord(Convert.ToDecimal(Tafkeet), new CurrencyInfo(CurrencyInfo.Currencies.EGP));
                        //    Tafkeet = toWord.ConvertToArabic();
                        //    Doc.SetParameterValue("Tafqeet", Tafkeet);
                        //}
                        //catch (Exception ex)
                        //{
                        //    Tafkeet = ex.ToString();
                        //}
                        if (MarketModel.DateCheck)
                        {
                            Doc.SetParameterValue("FromDate", MarketModel.FromDate.Date);
                            Doc.SetParameterValue("ToDate", MarketModel.ToDate.Date);
                        }
                        else
                        {
                            string Cmd2 = "Select Min(FromDate)As FromDate,Max(ToDate)As ToDate " +
                                        "From( " +
                                        "SELECT min(dat)AS fromDate, max(dat)AS toDate " +
                                        "FROM market " +
                                        "INNER JOIN doners " +
                                        "ON doners.id = market.name " +
                                        "INNER JOIN DonationPurpose " +
                                        "ON DonationPurpose.Id = market.DonationPurposeId " +
                                        "INNER JOIN BookResposibilities " +
                                        "ON market.ResponsibilityId = BookResposibilities.RespId " +
                                        "INNER JOIN HandleBookReceipts " +
                                        "ON HandleBookReceipts.BookReceiptId = BookResposibilities.HandleBookReceiptId " +
                                        "INNER JOIN BookTypes " +
                                        "ON BookTypes.BookTypeId = HandleBookReceipts.BookTypeId " +
                                        "INNER JOIN login " +
                                        "ON login.id = BookResposibilities.EmployeeId " +
                                        "INNER JOIN marketingsites " +
                                        "ON marketingsites.id = market.Site " +
                                        "INNER JOIN marketingrectype " +
                                        "ON marketingrectype.id = BookTypes.RecTypeId " +
                                        WCondition(MarketModel.UserNameCheck, MarketModel.DateCheck, MarketModel.RecTypeCheck, MarketModel.SiteCheck, MarketModel.PurposeCheck) +
                                        " Union " +
                                        "Select Min(CanceledReceipts.ActualDate), Max(CanceledReceipts.ActualDate) " +
                                        "FROM CanceledReceipts " +
                                        "Inner Join BookResposibilities " +
                                        "On dbo.BookResposibilities.RespId = dbo.CanceledReceipts.ResponsibilityId " +
                                        "Inner Join UserSites " +
                                        "On dbo.BookResposibilities.EmployeeId = dbo.UserSites.UserId " +
                                        "INNER JOIN login " +
                                        "ON login.id = BookResposibilities.EmployeeId " +
                                        "And UserSites.UserId = [login].id " +
                                        "Inner Join HandleBookReceipts " +
                                        "On BookResposibilities.HandleBookReceiptId = HandleBookReceipts.BookReceiptId " +
                                        "Inner Join BookTypes " +
                                        "On HandleBookReceipts.BookTypeId = BookTypes.BookTypeId " +
                                        "Inner Join marketingrectype " +
                                        "On marketingrectype.id = BookTypes.RecTypeId " +
                                        "Inner Join market " +
                                        "On BookResposibilities.RespId = dbo.market.ResponsibilityId " +
                                        CanceledWCondition(MarketModel.UserNameCheck, MarketModel.DateCheck, MarketModel.RecTypeCheck, MarketModel.SiteCheck) +
                                        " )T";
                            using (SqlCommand Com2 = new SqlCommand(Cmd2, Conn))
                            {
                                using (SqlDataReader Reader = Com2.ExecuteReader())
                                {
                                    if (Reader.Read())
                                    {
                                        try
                                        {
                                            Doc.SetParameterValue("FromDate", Reader.GetDateTime(0).Date.ToString());
                                            Doc.SetParameterValue("ToDate", Reader.GetDateTime(1).Date.ToString());
                                        }
                                        catch (Exception)
                                        {

                                            Doc.SetParameterValue("EmptyParam", "لا يوجد بيانات");
                                            Doc.SetParameterValue("FromDate", DateTime.Now.Date);
                                            Doc.SetParameterValue("ToDate", DateTime.Now.Date);
                                        }
                                    }
                                }
                            }
                        }
                    }
                    Stream stream = Doc.ExportToStream(ExportFormatType.PortableDocFormat);
                    stream.Seek(0, SeekOrigin.Begin);
                    return File(stream, "application/pdf");
                }
            }
        }
        private string WCondition(bool UserNameCheck,bool DateCheck,bool RecTypeCheck,bool SiteCheck,bool PurposeCheck)
        {
            string Condition = " ";
            int cbChecked = 0;
            if (UserNameCheck)
            {
                if (cbChecked > 0)
                    Condition += "and ";
                else
                    Condition += "where ";
                Condition += "BookResposibilities.EmployeeId = " + UserInfo.UserId + " ";

                cbChecked++;
            }
            if (RecTypeCheck)
            {
                if (cbChecked > 0)
                    Condition += "and ";
                else
                    Condition += "where ";
                Condition += "marketingrectype.id = " + RecTypeId + " ";

                cbChecked++;
            }
            if (SiteCheck)
            {
                if (cbChecked > 0)
                    Condition += "and ";
                else
                    Condition += "where ";
                Condition += "market.site = " + SiteId+ " ";

                cbChecked++;
            }
            if (DateCheck)
            {
                if (cbChecked > 0)
                    Condition += "and ";
                else
                    Condition += "where ";
                Condition += "dat >= '"+FromDate.Date+"'";
                Condition += " and ";
                Condition += "dat <= '"+ToDate.Date+"'";
                cbChecked++;
            }
            if (PurposeCheck)
            {
                if (cbChecked > 0)
                    Condition += "and ";
                else
                    Condition += "where ";
                Condition += "market.DonationPurposeId = "+PurposeId;
            }
            return Condition;
        }
        private string CanceledWCondition(bool UserNameCheck, bool DateCheck, bool RecTypeCheck, bool SiteCheck)
        {
            string Condition = " ";
            int cbChecked = 0;
            if (UserNameCheck)
            {
                if (cbChecked > 0)
                    Condition += "and ";
                else
                    Condition += "where ";
                Condition += "BookResposibilities.EmployeeId = " + UserInfo.UserId + " ";

                cbChecked++;
            }
            if (RecTypeCheck)
            {
                if (cbChecked > 0)
                    Condition += "and ";
                else
                    Condition += "where ";
                Condition += "marketingrectype.id = " + RecTypeId + " ";

                cbChecked++;
            }
            if (SiteCheck)
            {
                if (cbChecked > 0)
                    Condition += "and ";
                else
                    Condition += "where ";
                Condition += "market.site = " + SiteId + " ";

                cbChecked++;
            }
            if (DateCheck)
            {
                if (cbChecked > 0)
                    Condition += "and ";
                else
                    Condition += "where ";
                Condition += "Convert(Date,dbo.CanceledReceipts.ActualDate) >= '" + FromDate.Date + "'";
                Condition += " and ";
                Condition += "Convert(Date,dbo.CanceledReceipts.ActualDate) <= '" + ToDate.Date + "'";
                cbChecked++;
            }
            //if (PurposeCheck)
            //{
            //    if (cbChecked > 0)
            //        Condition += "and ";
            //    else
            //        Condition += "where ";
            //    Condition += "market.DonationPurposeId = " + PurposeId;
            //}
            return Condition;
        }
        private List<SelectListItem> PopulateUsers()
        {
            List<SelectListItem> Items = new List<SelectListItem>();
            using (MarketEntities db = new MarketEntities())
            {
                List<MarketGroupBriefModel> MyUser = (from S in db.Userlogins
                                                       where S.active ==1
                                             select new MarketGroupBriefModel() { UserId = S.id, UserName = S.FirstName+" "+S.LastName }).ToList<MarketGroupBriefModel>();
                foreach (MarketGroupBriefModel S in MyUser)
                {
                    SelectListItem selectList = new SelectListItem()
                    {
                        Text = S.UserName,
                        Value = S.UserId.ToString()
                    };
                    Items.Add(selectList);

                }
            }
            return Items;
        }
        public JsonResult PopulateRecTypes(int? UserId)
        {
            List<SelectListItem> Items = new List<SelectListItem>();
            using (MarketEntities context = new MarketEntities())
            {
                List<MarketGroupBriefModel> MyRecTypes = (from b in context.BookResposibilities
                                                                join h in context.HandleBookReceipts
                                                                on b.HandleBookReceiptId equals h.BookReceiptId
                                                                join t in context.BookTypes
                                                                on h.BookTypeId equals t.BookTypeId
                                                                join r in context.marketingrectypes
                                                                on t.RecTypeId equals r.id
                                                                where b.EmployeeId == UserId || UserId == null
                                                                select new MarketGroupBriefModel() { RecTypeId = r.id,
                                                                    RecType = r.name,BookNumber = t.BookNo}
                                                                ).OrderBy(x => x.RecType).ToList<MarketGroupBriefModel>();

                foreach (MarketGroupBriefModel rec in MyRecTypes)
                {
                    SelectListItem selectList = new SelectListItem()
                    {
                        Text = rec.RecType+"-"+rec.BookNumber.ToString(),
                        Value = rec.RecTypeId.ToString()
                    };
                    Items.Add(selectList);
                }
            }
            MarketGroupBriefModel MyRec = new MarketGroupBriefModel()
            {
                MyRecTypes = Items
            };
            return Json(MyRec.MyRecTypes, JsonRequestBehavior.AllowGet);
        }
        private List<SelectListItem> PopulateSites()
        {
            List<SelectListItem> Items = new List<SelectListItem>();
            using (MarketEntities db = new MarketEntities())
            {
                List<MarketGroupBriefModel> MySites = (from S in db.marketingsites
                                                            where S.Active == 1
                                                            select new MarketGroupBriefModel() {SiteId = S.id,SiteName =S.sitename}).ToList<MarketGroupBriefModel>();
                foreach (MarketGroupBriefModel S in MySites)
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
        private List<SelectListItem> PopulateDonationPurpose()
        {
            List<SelectListItem> Items = new List<SelectListItem>();
            using (MarketEntities db = new MarketEntities())
            {
                List<MarketGroupBriefModel> MyPurpose = (from S in db.DonationPurposes
                                                       where S.Active == 1
                                                       select new MarketGroupBriefModel() {DonationPurposeId = S.Id, DonationPurposeName = S.Purpose}).ToList<MarketGroupBriefModel>();
                foreach (MarketGroupBriefModel P in MyPurpose)
                {
                    SelectListItem selectList = new SelectListItem()
                    {
                        Text = P.DonationPurposeName,
                        Value = P.DonationPurposeId.ToString()
                    };
                    Items.Add(selectList);

                }
            }
            return Items;
        }
    }
}