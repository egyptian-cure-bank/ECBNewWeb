using ECBNewWeb.CustomAuthentication;
using ECBNewWeb.DataAccess;
using ECBNewWeb.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace ECBNewWeb.Controllers
{
    public class FinanceBookRequestsController : Controller
    {
        private CustomMembershipUser UserInfo;
        // GET: FinanceBookRequests
        public ActionResult BookRequests()
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
                                  where S.Active == 1 && D.SupervisorApproval == 1
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
                        Text = item.EmployeeNo.ToString()+"-"+item.RequestNo.ToString(),
                        Value = item.RequestId.ToString()
                    };
                    Items.Add(selectList);
                }
            }
            return Items;
        }
        public JsonResult GetRequestDetails (long RequestId)
        {
            using (MarketEntities db = new MarketEntities())
            {
                List<BookRequestModel> Data = (from h in db.BookRequests
                                               join d in db.BookRequestDetails on h.RequestNo equals d.RequestNo
                                               join e in db.Employees on h.EmployeeId equals e.EmployeeId
                                               join e2 in db.Employees on e.ParentEmployeeId equals e2.EmployeeId
                                               into p from parentemp in p.DefaultIfEmpty()
                                               join u in db.UserLogins on e.EmployeeId equals u.employee_id
                                               join s in db.UserSites on u.id equals s.UserId
                                               join t in db.marketingsites on s.SiteId equals t.id
                                               join r in db.marketingrectypes on d.ReceiptTypeId equals r.id
                                               where h.Active == 1 && s.Active ==1 && d.SupervisorApproval == 1 && d.FinanceApproval != 1 && h.RequestId == RequestId
                                               select new BookRequestModel {RequestNo = h.RequestNo, SiteName = t.sitename,EmployeeId = e.EmployeeId,
                                               FullEmployeeName = e.FirstName+" "+e.MiddleName+" "+e.LastName,
                                               RecTypeId = d.ReceiptTypeId,BookTypeName = r.name,Amount = d.Amount,
                                                   SupervisorName = parentemp.FirstName+" "+parentemp.MiddleName+" "+parentemp.LastName}
                                               ).ToList<BookRequestModel>();
                return Json(Data, JsonRequestBehavior.AllowGet);
            }
        }
        public JsonResult GetRequestMetaData (int RequestId,int[]RecTypeId,int[] Amount)
        {
            DataTable dt = new DataTable();
            SqlDataAdapter adapt = null;
            string JsonString = null;
            using (SqlConnection Conn = new SqlConnection(ConfigurationManager.ConnectionStrings["ECBConnectionString"].ConnectionString))
            {
                Conn.Open();
                using (SqlCommand Command = new SqlCommand("", Conn))
                {
                    if (RecTypeId != null)
                    {
                        for (var i = 0; i < RecTypeId.Length; i++)
                        {
                            Command.CommandText = "Declare @SumAmount int; " +
                                                "(Select @SumAmount = IsNull(Sum(BookRequestDetails.Amount), 0) From BookRequests Inner Join BookRequestDetails On BookRequestDetails.RequestNo = BookRequests.RequestNo " +
                                                "            Where (BookRequestDetails.SupervisorApproval != 1 And BookRequestDetails.FinanceApproval = 0) And dbo.BookRequests.RequestId < " + RequestId + " And BookRequestDetails.ReceiptTypeId = " + RecTypeId[i] + " ) " +
                                                "Select BookNo, HandleBookReceipts.BookReceiptId,BookTypes.RecTypeId,marketingrectype.[name],HandleBookReceipts.FirstReceiptNo,HandleBookReceipts.LastReceiptNo ,@SumAmount " +
                                                "FROM BookTypes " +
                                                "Inner Join HandleBookReceipts " +
                                                "On BookTypes.BookTypeId = HandleBookReceipts.BookTypeId " +
                                                "Inner Join marketingrectype " +
                                                "On BookTypes.RecTypeId = marketingrectype.id " +
                                                "Where Not Exists(Select 1 From BookResposibilities Where dbo.BookResposibilities.HandleBookReceiptId = dbo.HandleBookReceipts.BookReceiptId) " +
                                                "And BookTypes.RecTypeId = " + RecTypeId[i] +
                                                " And dbo.HandleBookReceipts.Active = 1 "+
                                                "Order By marketingrectype.[name],BookNo " +
                                                "OFFSET @SumAmount Rows " +
                                                "Fetch First " + Amount[i] + " Rows only";
                            //Command.Parameters["@BookTypeId"].Value = BookTypeId[i];
                            //Command.Parameters["@Amount"].Value = Amount[i];                            
                            adapt = new SqlDataAdapter(Command);
                            adapt.Fill(dt);
                        }
                    }
                    JsonSerializerSettings SerSettings = new JsonSerializerSettings();
                        SerSettings.Culture = System.Globalization.CultureInfo.InstalledUICulture;
                        JsonString = JsonConvert.SerializeObject(dt, SerSettings);
                }
            }
            return Json(JsonString, JsonRequestBehavior.AllowGet);

        }
        [HttpPost]
        public ActionResult ApproveBookRequest(int EmployeeId,int[] HandleBookReceiptId, int RequestId,int[] RecTypeId)
        {
            TempData["Msg"] = "";
            int InsertedRows = 0;
            int UpdatedRows = 0;
            using (SqlConnection Conn = new SqlConnection(ConfigurationManager.ConnectionStrings["ECBConnectionString"].ConnectionString))
            {
                Conn.Open();
                using (SqlCommand Com = new SqlCommand("", Conn))
                {
                    if (HandleBookReceiptId != null)
                    {
                        for (int i = 0; i < HandleBookReceiptId.Length; i++)
                        {
                            if (HandleBookReceiptId[i] != 0)
                            {
                                Com.CommandText = "Insert Into BookResposibilities " +
                                                    "(EmployeeId, HandleBookReceiptId, ReceiveDate, DoneFlag,RequestNo)Values(" + EmployeeId + "," + HandleBookReceiptId[i] + ",'" + DateTime.Now.Date + "', 0,(Select Distinct RequestNo From BookRequests Where RequestId ="+RequestId+"))";

                                InsertedRows += Com.ExecuteNonQuery();
                            }
                        }
                    }
                }
                using (SqlCommand ComUpdate = new SqlCommand("", Conn))
                {
                    if (RecTypeId != null)
                    {
                        for (int i = 0; i < RecTypeId.Length; i++)
                        {
                            ComUpdate.CommandText = "Update BookRequestDetails Set FinanceApproval = 1 Where RequestNo = (Select RequestNo From BookRequests " +
                                    "Where RequestId = " + RequestId + ") " +
                                    "And ReceiptTypeId = " + RecTypeId[i];
                            UpdatedRows += ComUpdate.ExecuteNonQuery();
                        }
                    }
                }
            }
            if (InsertedRows >= 0 || UpdatedRows >= 0)
            {
                TempData["Msg"] = "تم الحفظ بنجاح";
            }
            else
            {
                TempData["Msg"] = "لم يتم الحفظ";
            }
            return RedirectToAction("BookRequests");
        }
        private void MarkRequestAsApproved(long RequestNo, int RecTypeId)
        {

        }
    }
}