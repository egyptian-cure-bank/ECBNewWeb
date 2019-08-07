using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ECBNewWeb.Models;
using ECBNewWeb.DataAccess;
using System.Net;
using System.Data;
using ECBNewWeb.CustomAuthentication;
using System.Web.Security;
using System.Data.SqlClient;
using System.Configuration;
using Newtonsoft.Json;

namespace ECBNewWeb.Controllers
{
    public class BookResposibilitiesController : Controller
    {
        private CustomMembershipUser UserInfo;
        public ActionResult AllBookResponsibility()
        {
            if (User.Identity.IsAuthenticated)
            {
                UserInfo = (CustomMembershipUser)Membership.GetUser(HttpContext.User.Identity.Name, false);
                Session["CurrentUser"] = UserInfo.FirstName + " " + UserInfo.MiddleName + " " + UserInfo.LastName;
            }
            BookResposibilityModel _Model = new BookResposibilityModel();
            _Model.MyDeliveryNo = PopulateRequests();
            return View(_Model);
        }
        public JsonResult GetBookDeliveryRequests(int RequestId)
        {
            UserInfo = (CustomMembershipUser)Membership.GetUser(HttpContext.User.Identity.Name, false);
            DataTable dt = new DataTable();
            SqlDataAdapter adapt = null;
            string JsonString = null;
            string Cmd = "Select BookResposibilities.RespId,HandleBookReceipts.BookReceiptId,BookTypes.BookTypeId,BookRequests.RequestNo,BookDeliveryRequest.DeliveryId,BookDeliveryRequest.DeliveryNo,Employees.EmployeeNo,Employees.FirstName+' '+Employees.MiddleName+' '+Employees.LastName as FullEmpName, " +
                        "Departments.DepartmentName,BookDeliveryRequest.DeliveryDate,BookRequests.RequestDate,BookTypes.BookNo,marketingrectype.[name], " +
                        "Min(HandleBookReceipts.FirstReceiptNo) as FromReceipt,Max(HandleBookReceipts.LastReceiptNo) as ToReceipt, " +
                        "IsNull(BookResposibilities.NextReceiptNo,0)As NextReceiptNo, " +
                        "IsNull((Select Max(market.no) " +
                        "From market " +
                        "Where market.ResponsibilityId = min(BookResposibilities.RespId)),0)as LastSavedRecNo,IsNull(Max(CanceledReceipts.ReceiptNo),0) as LastCanceledReceiptNo," +
                        "marketingsites.sitename,ParentEmp.FirstName+' '+ParentEmp.MiddleName+' '+ParentEmp.LastName ParentEmpName " +
                        "From marketingrectype " +
                        "Inner Join BookTypes " +
                        "On BookTypes.RecTypeId = marketingrectype.id " +
                        "Inner Join HandleBookReceipts " +
                        "On HandleBookReceipts.BookTypeId = BookTypes.BookTypeId " +
                        "Inner Join BookResposibilities " +
                        "On BookResposibilities.HandleBookReceiptId = HandleBookReceipts.BookReceiptId " +
                        "Inner Join Employees " +
                        "On BookResposibilities.EmployeeId = Employees.EmployeeId " +
                        "Inner Join Departments " +
                        "On Employees.DepartmentId = Departments.DepartmentId " +
                        "Inner Join BookDeliveryRequestDetails " +
                        "On BookResposibilities.RespId = BookDeliveryRequestDetails.ResponsibilityId " +
                        "Inner Join BookDeliveryRequest " +
                        "On BookDeliveryRequest.DeliveryNo = BookDeliveryRequestDetails.DeliveryNo " +
                        "Inner Join BookRequests " +
                        "On BookDeliveryRequest.RequestId = BookRequests.RequestId " +
                        "Inner Join [login] " +
                        "On Employees.EmployeeId = [login].employee_id " +
                        "Inner Join UserSites " +
                        "On [login].id = UserSites.UserId " +
                        "Inner Join marketingsites " +
                        "On UserSites.SiteId = marketingsites.id " +
                        "Inner Join Employees ParentEmp " +
                        "On ParentEmp.EmployeeId = Employees.ParentEmployeeId " +
                        "Left Join CanceledReceipts " +
                        "On BookResposibilities.RespId = dbo.CanceledReceipts.ResponsibilityId " +
                        "Where (BookResposibilities.DeliveryDate Is Null Or BookResposibilities.DeliveryDate = '') " +
                        "And BookDeliveryRequest.RequestId = @RequestId " +
                        "Group By BookResposibilities.RespId,HandleBookReceipts.BookReceiptId,BookTypes.BookTypeId,BookTypes.BookNo,BookRequests.RequestDate,marketingrectype.[name],BookDeliveryRequest.DeliveryId,Employees.EmployeeNo,Employees.FirstName,Employees.MiddleName,Employees.LastName,Departments.DepartmentName, " +
                        "BookDeliveryRequest.DeliveryNo,BookDeliveryRequest.DeliveryDate,BookResposibilities.NextReceiptNo,BookRequests.RequestNo,marketingsites.sitename ,ParentEmp.FirstName,ParentEmp.MiddleName,ParentEmp.LastName " +
                        "Order By marketingrectype.[name]"; ;
            using (SqlConnection Conn = new SqlConnection(ConfigurationManager.ConnectionStrings["ECBConnectionString"].ConnectionString))
            {
                Conn.Open();
                using (SqlCommand Command = new SqlCommand(Cmd, Conn))
                {
                    Command.Parameters.AddWithValue("@RequestId", RequestId);
                    adapt = new SqlDataAdapter(Command);
                    adapt.Fill(dt);
                    JsonSerializerSettings SerSettings = new JsonSerializerSettings();
                    SerSettings.Culture = System.Globalization.CultureInfo.InstalledUICulture;
                    JsonString = JsonConvert.SerializeObject(dt, SerSettings);
                }
            }
            return Json(JsonString, JsonRequestBehavior.AllowGet);

        }
        [HttpPost]
        public ActionResult ApproveDeliveryRequest(BookResposibilityModel model)
        {
            int rowAffected = 0;
            int insertedRows = 0;
            MarketEntities db = new MarketEntities();
            BookResposibility BookRespToUpdate = new BookResposibility();
            BookDeliveryRequestDetail DeliveryRequestDetailUpdate = new BookDeliveryRequestDetail();
            HandleBookReceipt HbookReceipt = new HandleBookReceipt();
            //Update Resp
            BookRespToUpdate = db.BookResposibilities.Find(model.RespId);
            BookRespToUpdate.DeliveryDate = DateTime.Now;
            BookRespToUpdate.DoneFlag = 1;
            //Update BookeDeliverydetails
            DeliveryRequestDetailUpdate = (from d in db.BookDeliveryRequestDetails
                                           where d.ResponsibilityId == model.RespId
                                           select d).FirstOrDefault();
            DeliveryRequestDetailUpdate.FinanceApproval = 1;
            rowAffected = db.SaveChanges();
            if (model.BookState == "غير منتهي")
            {
                if (model.NextReceiptNo == 0 )
                {
                    HbookReceipt.BookTypeId = model.BookTypeId;
                    HbookReceipt.FirstReceiptNo = model.FirstReceiptNo;
                    HbookReceipt.LastReceiptNo = model.LastReceiptNo;
                    HbookReceipt.ParentBookReceiptId = model.HandleBookReceiptId;
                    HbookReceipt.Active = 1;
                    db.HandleBookReceipts.Add(HbookReceipt);
                    insertedRows = db.SaveChanges();
                    //Mark HandleBook as Inactive
                    HbookReceipt = db.HandleBookReceipts.Find(model.HandleBookReceiptId);
                    HbookReceipt.Active = 0;
                    rowAffected = db.SaveChanges();
                }
                else
                {
                    HbookReceipt.BookTypeId = model.BookTypeId;
                    HbookReceipt.FirstReceiptNo = model.NextReceiptNo;
                    HbookReceipt.LastReceiptNo = model.LastReceiptNo;
                    HbookReceipt.ParentBookReceiptId = model.HandleBookReceiptId;
                    HbookReceipt.Active = 1;
                    db.HandleBookReceipts.Add(HbookReceipt);
                    insertedRows = db.SaveChanges();
                    //Mark HandleBook as Inactive
                    HbookReceipt = db.HandleBookReceipts.Find(model.HandleBookReceiptId);
                    HbookReceipt.Active = 0;
                    rowAffected = db.SaveChanges();
                }
            }
            if (rowAffected > 0 && insertedRows > 0 )
            {
                TempData["Msg"] = "تم الحفظ بنجاح";
            }
            else
            {
                TempData["Msg"] = "لم يتم الحفظ";
            }
            return RedirectToAction("AllBookResponsibility") ;
        }
        private List<SelectListItem> PopulateRequests()
        {
            UserInfo = (CustomMembershipUser)Membership.GetUser(HttpContext.User.Identity.Name, false);
            List<SelectListItem> Items = new List<SelectListItem>();
            List<BookDeliveryModel> MyRequests;
            using (MarketEntities db = new MarketEntities())
            {
                var AllAdmins = (from E in db.Employees
                                 join U in db.UserLogins on E.EmployeeId equals U.employee_id
                                 where E.Active == 1 && U.active == 1 && E.ParentEmployeeId == null && E.EmployeeId == UserInfo.EmployeeId
                                 select E).FirstOrDefault();
                if (AllAdmins != null)
                {
                    MyRequests = (from B in db.BookDeliveryRequests
                                  join H in db.BookRequests on B.RequestId equals H.RequestId
                                  join D in db.BookDeliveryRequestDetails on B.DeliveryNo equals D.DeliveryNo
                                  join E in db.Employees on B.EmployeeId equals E.EmployeeId
                                  where D.SupervisorApproval == 1 && D.FinanceApproval != 1
                                  select new BookDeliveryModel() { RequestId = H.RequestId, DeliveryNo = B.DeliveryNo, EmployeeNo = E.EmployeeNo }).Distinct().OrderByDescending(order => order.DeliveryNo).ToList<BookDeliveryModel>();
                }
                else
                {
                    MyRequests = (from B in db.BookDeliveryRequests
                                  join H in db.BookRequests on B.RequestId equals H.RequestId
                                  join D in db.BookDeliveryRequestDetails on B.DeliveryNo equals D.DeliveryNo
                                  join E in db.Employees on B.EmployeeId equals E.EmployeeId
                                  where D.SupervisorApproval == 1 && D.FinanceApproval != 1
                                  select new BookDeliveryModel() { RequestId = H.RequestId, DeliveryNo = B.DeliveryNo, EmployeeNo = E.EmployeeNo }).Distinct().OrderByDescending(order => order.DeliveryNo).ToList<BookDeliveryModel>();
                }
                foreach (BookDeliveryModel item in MyRequests)
                {
                    SelectListItem selectList = new SelectListItem()
                    {
                        Text = item.EmployeeNo.ToString() + "-" + item.DeliveryNo.ToString(),
                        Value = item.RequestId.ToString()
                    };
                    Items.Add(selectList);
                }
            }
            return Items;
        }
        public JsonResult GetNextReceiptNoFromRecType(int RecTypeId)
        {
            UserInfo = (CustomMembershipUser)Membership.GetUser(HttpContext.User.Identity.Name, false);
            DataTable dt = new DataTable();
            string Cmd = "Select min(BookResp.HandleBookReceiptId)as HBookRecId, " +
                        "min(BookResp.NextReceiptNo) as NextReceiptNo, " +
                        "(Select Max(market.no) " +
                        "From market " +
                        "Where market.ResponsibilityId = min(BookResp.RespId))as LastSavedRecNo, " +
                        "Max(CanceledReceipts.ReceiptNo) as LastCanceledReceiptNo, " +
                        "Min(HandleBookReceipts.LastReceiptNo) as LastReceiptNo " +
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
    }
}