using CrystalDecisions.CrystalReports.Engine;
using CrystalDecisions.Shared;
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
    public class PrintBookRequestsController : Controller
    {
        private CustomMembershipUser UserInfo;
        public ActionResult PrintRequests()
        {
            if (User.Identity.IsAuthenticated)
            {
                UserInfo = (CustomMembershipUser)Membership.GetUser(HttpContext.User.Identity.Name, false);
                Session["CurrentUser"] = UserInfo.FirstName + " " + UserInfo.MiddleName + " " + UserInfo.LastName;
            }
            BookRequestModel _ReqModel = new BookRequestModel();
            _ReqModel.MyRequests = PopulateRequests();
            return View(_ReqModel);
        }
        private List<SelectListItem> PopulateRequests()
        {
            List<BookRequestModel> MyRequests;
            List<SelectListItem> Items = new List<SelectListItem>();
            using (MarketEntities db = new MarketEntities())
            {
                var AllAdmins = (from E in db.Employees
                                 join U in db.UserLogins on E.EmployeeId equals U.employee_id
                                 where E.Active == 1 && U.active == 1 && E.ParentEmployeeId == null && E.EmployeeId == UserInfo.EmployeeId
                                 select E).FirstOrDefault();
                if (AllAdmins != null)
                {
                    MyRequests = (from S in db.BookRequests
                                  join D in db.BookRequestDetails on S.RequestNo equals D.RequestNo
                                  join E in db.Employees on S.EmployeeId equals E.EmployeeId
                                  where S.Active == 1 && D.FinanceApproval != 1 && D.SupervisorApproval == 1
                                  select new BookRequestModel() { RequestId = S.RequestId, RequestNo = S.RequestNo, EmployeeNo = E.EmployeeNo }).Distinct().OrderByDescending(order => order.RequestNo).ToList<BookRequestModel>();
                }
                else
                {
                    MyRequests = (from S in db.BookRequests
                                  join D in db.BookRequestDetails on S.RequestNo equals D.RequestNo
                                  join E in db.Employees on S.EmployeeId equals E.EmployeeId
                                  where S.Active == 1 && D.FinanceApproval != 1 && D.SupervisorApproval == 1
                                  && (E.ParentEmployeeId == UserInfo.EmployeeId)
                                  select new BookRequestModel() { RequestId = S.RequestId, RequestNo = S.RequestNo, EmployeeNo = E.EmployeeNo }).Distinct().OrderByDescending(order => order.RequestNo).ToList<BookRequestModel>();
                }
                foreach (BookRequestModel item in MyRequests)
                {
                    SelectListItem selectList = new SelectListItem()
                    {
                        Text = item.EmployeeNo.ToString() + "-" + item.RequestNo.ToString(),
                        Value = item.RequestId.ToString()
                    };
                    Items.Add(selectList);
                }
            }
            return Items;
        }
        public ActionResult PrintReport(FormCollection form)
        {
            string Cmd = "Select BookRequests.EmployeeId, BookRequests.RequestDate, BookRequests.RequestNo, "+
                            "Employees.FirstName + ' ' + Employees.MiddleName + ' ' + Employees.LastName As EmpFullName, Departments.DepartmentName,marketingsites.sitename, "+
                            "marketingrectype.[name]As RecType, BookRequestDetails.Amount, "+
                            "IsNull((Select Top 1 InnerBookReq.RequestNo From BookRequests InnerBookReq "+
                            "        Where BookRequests.EmployeeId = [login].employee_id And InnerBookReq.RequestId < BookRequests.RequestId "+
                            "        Order by InnerBookReq.RequestId Desc),0) As BeforeLatestRequest "+
                            "From BookRequests "+
                            "Inner Join BookRequestDetails "+
                            "On BookRequestDetails.RequestNo = BookRequests.RequestNo "+
                            "Inner Join marketingrectype "+
                            "On BookRequestDetails.ReceiptTypeId = marketingrectype.id "+
                            "Inner Join Employees "+
                            "On BookRequests.EmployeeId = Employees.EmployeeId "+
                            "Inner Join Departments "+
                            "On Employees.DepartmentId = Departments.DepartmentId "+
                            "Inner Join[login] "+
                            "On[login].employee_id = Employees.EmployeeId "+
                            "Inner Join UserSites "+
                            "On[login].id = UserSites.UserId "+
                            "Inner Join marketingsites "+
                            "On dbo.UserSites.SiteId = marketingsites.id "+
                            "Where dbo.BookRequestDetails.FinanceApproval = 1 "+
                            "And dbo.BookRequestDetails.SupervisorApproval = 1 "+
                            "And UserSites.Active = 1 "+
                            "And BookRequests.RequestId = @RequestId";
            using (SqlConnection Conn = new SqlConnection(ConfigurationManager.ConnectionStrings["ECBConnectionString"].ConnectionString))
            {
                Conn.Open();
                using (SqlCommand Com = new SqlCommand(Cmd, Conn))
                {
                    Com.Parameters.AddWithValue("@RequestId", Convert.ToInt32(form["RequestId"].ToString()));
                    SqlDataAdapter Adapt = new SqlDataAdapter(Com);
                    DataTable dt = new DataTable();
                    Adapt.Fill(dt);
                    ReportDocument Doc = new ReportDocument();
                    Doc.Load(Path.Combine(Server.MapPath("~/CrystalReports/Marketing"), "rptMarketingBookRequest.rpt"));
                    Doc.SetDataSource(dt);
                    if (dt.Rows.Count > 0)
                    {
                        try
                        {
                            Stream stream = Doc.ExportToStream(ExportFormatType.PortableDocFormat);
                            stream.Seek(0, SeekOrigin.Begin);
                            return File(stream, "application/pdf");
                        }
                        catch (Exception)
                        {

                        }
                    }
                }
            }
            return RedirectToAction("PrintRequests");
        }
        public ActionResult PrintReceiveReport(FormCollection form)
        {
            string Cmd = "Select Distinct BookRequests.EmployeeId, BookRequests.RequestDate ,BookResposibilities.ReceiveDate, BookRequests.RequestNo,  "+
                            "Employees.FirstName + ' ' + Employees.MiddleName + ' ' + Employees.LastName As EmpFullName, Departments.DepartmentName,marketingsites.sitename,  "+
                            "marketingrectype.[name]As RecType, Count(distinct BookResposibilities.RespId)As Amount, " +
                            "Min(BookTypes.BookNo)FromBook, Max(BookTypes.BookNo)ToBook, Min(HandleBookReceipts.FirstReceiptNo) as FromReceipt, Max(HandleBookReceipts.LastReceiptNo) as ToReceipt, "+
                            "(Select Max(market.no) "+
                            "From market "+
                            "Where market.ResponsibilityId = min(BookResposibilities.RespId)) as LastSavedRecNo, Max(CanceledReceipts.ReceiptNo) as LastCanceledReceiptNo "+
                            "From BookRequests "+
                            "Inner Join BookResposibilities "+
                            "On BookRequests.RequestNo = BookResposibilities.RequestNo "+
                            "Inner Join HandleBookReceipts "+
                            "On BookResposibilities.HandleBookReceiptId = HandleBookReceipts.BookReceiptId "+
                            "Left Join HandleBookReceipts ParentReceipt "+
                            "On ParentReceipt.ParentBookReceiptId = HandleBookReceipts.BookReceiptId "+
                            "Inner Join BookTypes "+
                            "On HandleBookReceipts.BookTypeId = BookTypes.BookTypeId "+
                            "Left Join CanceledReceipts "+
                            "On BookResposibilities.RespId = CanceledReceipts.ResponsibilityId "+
                            "Inner Join marketingrectype "+
                            "On BookTypes.RecTypeId = marketingrectype.id "+
                            "Inner Join Employees "+
                            "On BookRequests.EmployeeId = Employees.EmployeeId "+
                            "Inner Join Departments "+
                            "On Employees.DepartmentId = Departments.DepartmentId "+
                            "Inner Join[login] "+
                            "On[login].employee_id = Employees.EmployeeId "+
                            "Inner Join UserSites "+
                            "On[login].id = UserSites.UserId "+
                            "Inner Join marketingsites "+
                            "On dbo.UserSites.SiteId = marketingsites.id "+
                            "Where UserSites.Active = 1 "+
                            "And BookRequests.RequestId = @RequestId "+
                            "Group by BookRequests.EmployeeId, BookRequests.RequestDate, BookRequests.RequestNo, "+
                            "Employees.FirstName , Employees.MiddleName, Employees.LastName, Departments.DepartmentName,marketingsites.sitename, "+
                            "marketingrectype.[name],BookResposibilities.ReceiveDate";

            using (SqlConnection Conn = new SqlConnection(ConfigurationManager.ConnectionStrings["ECBConnectionString"].ConnectionString))
            {
                Conn.Open();
                using (SqlCommand Com = new SqlCommand(Cmd, Conn))
                {
                    Com.Parameters.AddWithValue("@RequestId", Convert.ToInt32(form["RequestId"].ToString()));
                    SqlDataAdapter Adapt = new SqlDataAdapter(Com);
                    DataTable dt = new DataTable();
                    Adapt.Fill(dt);
                    ReportDocument Doc = new ReportDocument();
                    Doc.Load(Path.Combine(Server.MapPath("~/CrystalReports/Marketing"), "rptMarketingReceiveBook.rpt"));
                    Doc.SetDataSource(dt);
                    if (dt.Rows.Count > 0)
                    {
                        try
                        {
                            Stream stream = Doc.ExportToStream(ExportFormatType.PortableDocFormat);
                            stream.Seek(0, SeekOrigin.Begin);
                            return File(stream, "application/pdf");
                        }
                        catch (Exception)
                        {

                        }
                    }
                }
            }
            return RedirectToAction("PrintRequests");
        }
    }
}