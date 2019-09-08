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
    public class BookDeliveryRequestsController : Controller
    {
        private CustomMembershipUser UserInfo;
        // GET: BookDeliveryRequests
        public ActionResult CreateBookDeliveryRequests()
        {
            if (User.Identity.IsAuthenticated)
            {
                UserInfo = (CustomMembershipUser)Membership.GetUser(HttpContext.User.Identity.Name, false);
                Session["CurrentUser"] = UserInfo.FirstName + " " + UserInfo.MiddleName + " " + UserInfo.LastName;
            }
            BookDeliveryModel _Model = new BookDeliveryModel();
            _Model.MyRequests = PopulateRequests();
            return View(_Model);
        }
        private List<SelectListItem> PopulateRequests()
        {
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
                    MyRequests = (from S in db.BookRequests
                                  join D in db.BookRequestDetails on S.RequestNo equals D.RequestNo
                                  join E in db.Employees on S.EmployeeId equals E.EmployeeId
                                  where S.Active == 1 && D.FinanceApproval == 1 && D.SupervisorApproval == 1
                                  select new BookDeliveryModel() { RequestId = S.RequestId, RequestNo = S.RequestNo, EmployeeNo = E.EmployeeNo }).Distinct().OrderByDescending(order => order.RequestNo).ToList<BookDeliveryModel>();
                }
                else
                {
                    MyRequests = (from S in db.BookRequests
                                  join D in db.BookRequestDetails on S.RequestNo equals D.RequestNo
                                  join E in db.Employees on S.EmployeeId equals E.EmployeeId
                                  where S.Active == 1 && D.FinanceApproval == 1 && D.SupervisorApproval == 1
                                  && (E.EmployeeId == UserInfo.EmployeeId)
                                  select new BookDeliveryModel() { RequestId = S.RequestId, RequestNo = S.RequestNo, EmployeeNo = E.EmployeeNo }).Distinct().OrderByDescending(order => order.RequestNo).ToList<BookDeliveryModel>();
                }
                foreach (BookDeliveryModel item in MyRequests)
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
        public JsonResult PopulateRecType(int RequestId)
        {
            UserInfo = (CustomMembershipUser)Membership.GetUser(HttpContext.User.Identity.Name, false);
            List<SelectListItem> Items = new List<SelectListItem>();
            using (MarketEntities db = new MarketEntities())
            {
                List<BookDeliveryModel> MyRecType = (from M in db.marketingrectypes
                                                  join B in db.BookTypes on M.id equals B.RecTypeId
                                                  join H in db.HandleBookReceipts on B.BookTypeId equals H.BookTypeId
                                                  join R in db.BookResposibilities on H.BookReceiptId equals R.HandleBookReceiptId
                                                  join Q in db.BookRequests on R.RequestNo equals Q.RequestNo
                                                  where R.DoneFlag == 0 && Q.RequestId == RequestId
                                             select new BookDeliveryModel() { RecTypeName = M.name,RecTypeId=M.id }).Distinct().ToList<BookDeliveryModel>();
                foreach (BookDeliveryModel Book in MyRecType)
                {
                    SelectListItem selectList = new SelectListItem()
                    {
                        Text =Book.RecTypeName,
                        Value = Book.RecTypeId.ToString(),
                    };
                    Items.Add(selectList);
                }
            }
            return Json(Items, JsonRequestBehavior.AllowGet);
        }
        public JsonResult GetBookNumberFromRecType(int RecTypeId,int RequestId)
        {
            UserInfo = (CustomMembershipUser)Membership.GetUser(HttpContext.User.Identity.Name, false);
            DataTable dt = new DataTable();
            string JsonString = null;
            string Cmd = "Select BookResposibilities.RespId,BookTypes.BookTypeId,BookTypes.BookNo "+
                        "From marketingrectype "+
                        "Inner Join BookTypes "+
                        "On BookTypes.RecTypeId = marketingrectype.id "+
                        "Inner Join HandleBookReceipts "+
                        "On HandleBookReceipts.BookTypeId = BookTypes.BookTypeId "+
                        "Inner Join BookResposibilities "+
                        "On BookResposibilities.HandleBookReceiptId = HandleBookReceipts.BookReceiptId "+
                        "Inner Join BookRequests "+
                        "On BookRequests.RequestNo = BookResposibilities.RequestNo "+
                        "Where Not Exists(Select 1 From BookDeliveryRequestDetails Where ResponsibilityId = BookResposibilities.RespId) "+
                        "And BookResposibilities.DoneFlag = 0 "+
                        "And marketingrectype.id = @RecTypeId " +
                        "And BookRequests.RequestId = @RequestId "+
                        "Order By BookTypes.BookNo";
            using (SqlConnection Conn = new SqlConnection(ConfigurationManager.ConnectionStrings["ECBConnectionString"].ConnectionString))
            {
                Conn.Open();
                using (SqlCommand Command = new SqlCommand(Cmd, Conn))
                {
                    Command.Parameters.AddWithValue("@RecTypeId", RecTypeId);
                    //Command.Parameters.AddWithValue("@EmployeeId", UserInfo.EmployeeId);
                    Command.Parameters.AddWithValue("@RequestId", RequestId);
                    SqlDataAdapter adapt = new SqlDataAdapter(Command);
                    adapt.Fill(dt);
                    JsonSerializerSettings SerSettings = new JsonSerializerSettings();
                    SerSettings.Culture = System.Globalization.CultureInfo.InstalledUICulture;
                    JsonString = JsonConvert.SerializeObject(dt, SerSettings);
                }
            }
            return Json(JsonString, JsonRequestBehavior.AllowGet);
        }
        public JsonResult GetInfoDetails(int[] RespIds)
        {
            UserInfo = (CustomMembershipUser)Membership.GetUser(HttpContext.User.Identity.Name, false);
            DataTable dt = new DataTable();
            SqlDataAdapter adapt = null;
            string JsonString = null;
            string Cmd = string.Format("Select marketingrectype.[name],Min(HandleBookReceipts.FirstReceiptNo)as FromReceipt,Max(HandleBookReceipts.LastReceiptNo)as ToReceipt, "+
                            "(Select Max(market.no) "+
                            "From market "+
                            "Where market.ResponsibilityId = min(BookResposibilities.RespId)) as LastSavedRecNo, Max(CanceledReceipts.ReceiptNo) as LastCanceledReceiptNo "+
                            "From marketingrectype "+
                            "Inner Join BookTypes "+
                            "On BookTypes.RecTypeId = marketingrectype.id "+
                            "Inner Join HandleBookReceipts "+
                            "On HandleBookReceipts.BookTypeId = BookTypes.BookTypeId "+
                            "Inner Join BookResposibilities "+
                            "On BookResposibilities.HandleBookReceiptId = HandleBookReceipts.BookReceiptId "+
                            "Left Join CanceledReceipts "+
                            "On BookResposibilities.RespId = CanceledReceipts.ResponsibilityId "+
                            "Where BookResposibilities.DoneFlag = 0 "+
                            "And HandleBookReceipts.Active = 1 " +
                            "And BookResposibilities.RespId in ({0}) "+
                            "Group By marketingrectype.[name] "+ 
                            "Order By marketingrectype.[name]",string.Join(",",RespIds));
            using (SqlConnection Conn = new SqlConnection(ConfigurationManager.ConnectionStrings["ECBConnectionString"].ConnectionString))
            {
                Conn.Open();
                using (SqlCommand Command = new SqlCommand(Cmd, Conn))
                {
                    //Command.Parameters.AddWithValue("@EmployeeId", UserInfo.EmployeeId);
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
        public ActionResult SaveBookDeliveryRequest(int[]RespIds,int RequestId)
        {
            UserInfo = (CustomMembershipUser)Membership.GetUser(HttpContext.User.Identity.Name, false);
            long? MaxDeliveryNo = 0;
            int InsertedRows = 0;
            if (RespIds != null)
            {
                //save To Header Table
                MarketEntities db = new MarketEntities();
                using (SqlConnection Conn = new SqlConnection(ConfigurationManager.ConnectionStrings["ECBConnectionString"].ConnectionString))
                {
                    Conn.Open();
                    int Result = 0;
                    //Check If There is Delivery Request Pending
                    string CheckCmd = "Select Count(BookRequests.RequestId) "+
                                        "From BookRequests "+
                                        "Inner Join BookDeliveryRequest "+
                                        "On BookDeliveryRequest.RequestId = BookRequests.RequestId "+
                                        "Inner Join BookDeliveryRequestDetails "+
                                        "On BookDeliveryRequest.DeliveryNo = BookDeliveryRequestDetails.DeliveryNo "+
                                        "Where BookDeliveryRequest.EmployeeId = @EmployeeId " +
                                        "And BookDeliveryRequestDetails.FinanceApproval = 0 " +
                                        "And BookDeliveryRequestDetails.SupervisorApproval = 0";
                    
                    using (SqlCommand CheckCommand = new SqlCommand(CheckCmd,Conn))
                    {
                        CheckCommand.Parameters.AddWithValue("@EmployeeId", UserInfo.EmployeeId);
                        Result = (Int32)CheckCommand.ExecuteScalar();
                    }
                    if (Result > 0)//There is a delivery request pending
                    {
                        Session["Msg"] = "PendingRequest";
                    }
                    else
                    {
                        using (SqlCommand Com = new SqlCommand("", Conn))
                        {
                            Com.CommandText = "Insert Into BookDeliveryRequest (DeliveryNo,EmployeeId,DeliveryDate,RequestId) Values " +
                                "(Next Value For SequenceDeliveryNo,@EmployeeId,@DeliveryDate,@RequestId)";
                            Com.Parameters.AddWithValue("@EmployeeId", UserInfo.EmployeeId);
                            Com.Parameters.AddWithValue("@DeliveryDate", DateTime.Now);
                            Com.Parameters.AddWithValue("@RequestId", RequestId);
                            InsertedRows = Com.ExecuteNonQuery();
                        }
                        MaxDeliveryNo = db.BookDeliveryRequests.Where(y => y.EmployeeId == UserInfo.EmployeeId).Max(x => x.DeliveryNo);
                        InsertedRows = SaveToDetails(RespIds, MaxDeliveryNo);
                        if (InsertedRows > 0)
                        {
                            Session["Msg"] = "تم الحفظ بنجاح";
                        }
                        else
                        {
                            Session["Msg"] = "لم يتم الحفظ";
                        }
                    }
                }

            }
            else
            {
                Session["Msg"] = "لم يتم الحفظ";
            }
            return RedirectToAction("CreateBookDeliveryRequests");
        }
        private int SaveToDetails(int[] Resps, long? MaxDeliveryNo)
        {
            int InsertedRows = 0;
            //save To Details
            using (SqlConnection Conn = new SqlConnection(ConfigurationManager.ConnectionStrings["ECBConnectionString"].ConnectionString))
            {
                Conn.Open();
                using (SqlCommand Com = new SqlCommand("", Conn))
                {

                    for (int i = 0; i < Resps.Length; i++)
                    {
                        Com.CommandText = "Insert Into BookDeliveryRequestDetails(DeliveryNo,ResponsibilityId,FinanceApproval,SupervisorApproval) Values(" + MaxDeliveryNo + "," + Resps[i] + ",0,0)";
                        InsertedRows = Com.ExecuteNonQuery();
                    }
                }
            }
            return InsertedRows;
        }
        public List<SelectListItem> PopulateRequestHasDelivery()
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
                                  where D.SupervisorApproval != 1 && D.FinanceApproval != 1
                                  select new BookDeliveryModel() { RequestId = H.RequestId, DeliveryNo = B.DeliveryNo, EmployeeNo = E.EmployeeNo }).Distinct().OrderByDescending(order => order.DeliveryNo).ToList<BookDeliveryModel>();
                }
                else
                {
                    MyRequests = (from B in db.BookDeliveryRequests
                                  join H in db.BookRequests on B.RequestId equals H.RequestId
                                  join D in db.BookDeliveryRequestDetails on B.DeliveryNo equals D.DeliveryNo
                                  join E in db.Employees on B.EmployeeId equals E.EmployeeId
                                  where E.EmployeeId == UserInfo.EmployeeId && D.SupervisorApproval != 1 && D.FinanceApproval != 1
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
        public ActionResult AllBookDeliveryRequests()
        {
            BookDeliveryModel _Model = new BookDeliveryModel();
            _Model.MyDeliveryNo = PopulateRequestHasDelivery();
            return View(_Model);
        }
        public JsonResult GetBookDeliveryRequests(int RequestId)
        {
            UserInfo = (CustomMembershipUser)Membership.GetUser(HttpContext.User.Identity.Name, false);
            DataTable dt = new DataTable();
            SqlDataAdapter adapt = null;
            string JsonString = null;
            string Cmd = "Select BookRequests.RequestNo,BookDeliveryRequest.DeliveryId,BookDeliveryRequest.DeliveryNo,Employees.EmployeeNo,Employees.FirstName+' '+Employees.MiddleName+' '+Employees.LastName as FullEmpName, " +
                          "Departments.DepartmentName,BookDeliveryRequest.DeliveryDate,marketingrectype.[name],Count(distinct BookResposibilities.RespId) BookCount, "+
                          "Min(BookTypes.BookNo)FromBook,Max(BookTypes.BookNo)ToBook,Min(HandleBookReceipts.FirstReceiptNo) as FromReceipt,Max(HandleBookReceipts.LastReceiptNo) as ToReceipt "+
                          "From marketingrectype "+
                          "Inner Join BookTypes "+
                          "On BookTypes.RecTypeId = marketingrectype.id "+
                          "Inner Join HandleBookReceipts "+
                          "On HandleBookReceipts.BookTypeId = BookTypes.BookTypeId "+
                          "Inner Join BookResposibilities "+
                          "On BookResposibilities.HandleBookReceiptId = HandleBookReceipts.BookReceiptId "+
                          "Inner Join Employees "+
                          "On BookResposibilities.EmployeeId = Employees.EmployeeId "+
                          "Inner Join Departments "+
                          "On Employees.DepartmentId = Departments.DepartmentId "+
                          "Inner Join BookDeliveryRequestDetails "+
                          "On BookResposibilities.RespId = BookDeliveryRequestDetails.ResponsibilityId "+
                          "Inner Join BookDeliveryRequest "+
                          "On BookDeliveryRequest.DeliveryNo = BookDeliveryRequestDetails.DeliveryNo "+
                          "Inner Join BookRequests "+
                          "On BookDeliveryRequest.RequestId = BookRequests.RequestId "+
                          "Where BookResposibilities.DoneFlag = 0 "+ 
                          "And BookDeliveryRequest.RequestId = @RequestId "+
                          "Group By marketingrectype.[name],BookDeliveryRequest.DeliveryId,Employees.EmployeeNo,Employees.FirstName,Employees.MiddleName,Employees.LastName,Departments.DepartmentName, " +
                          "BookDeliveryRequest.DeliveryNo,BookDeliveryRequest.DeliveryDate,BookRequests.RequestNo " +
                          "Order By marketingrectype.[name]";
            using (SqlConnection Conn = new SqlConnection(ConfigurationManager.ConnectionStrings["ECBConnectionString"].ConnectionString))
            {
                Conn.Open();
                using (SqlCommand Command = new SqlCommand(Cmd, Conn))
                {
                    Command.Parameters.AddWithValue("@EmployeeId", UserInfo.EmployeeId);
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
        public ActionResult DeleteDeliveryRequest(int RequestId)
        {
            long DeliveryNo = 0;
            int DeletedRecords = 0;
            int ApprovedRequest = 0;
            string DeliveryNoCmd = "Select DeliveryNo From BookDeliveryRequest Where RequestId = @RequestId";
            string Cmd = "Delete From BookDeliveryRequest Where RequestId = @RequestId";
            using (SqlConnection Conn = new SqlConnection(ConfigurationManager.ConnectionStrings["ECBConnectionString"].ConnectionString))
            {
                Conn.Open();
                using (SqlCommand Command = new SqlCommand(DeliveryNoCmd,Conn))
                {
                    Command.Parameters.AddWithValue("@RequestId",RequestId);
                    SqlDataReader Reader = Command.ExecuteReader();
                    if (Reader.HasRows)
                    {
                        if (Reader.Read())
                        {
                            DeliveryNo = Reader.GetInt64(0);
                            Reader.Close();
                            Command.CommandText = "Select Count(DeliveryNo) From BookDeliveryRequestDetails Where DeliveryNo = @DeliveryNo "+
                                                    "And (FinanceApproval = 1 Or SupervisorApproval = 1) ";
                            Command.Parameters.AddWithValue("@DeliveryNo", DeliveryNo);
                            ApprovedRequest = (Int32)Command.ExecuteScalar();
                        }
                        if (ApprovedRequest > 0)//Request Approved And Cannot Deleted
                        {
                            Session["dMsg"] = "RequestApproved";
                        }
                        else
                        {
                            //Delete From Header
                            using (SqlCommand Command2 = new SqlCommand(Cmd, Conn))
                            {
                                Command2.Parameters.AddWithValue("@RequestId", RequestId);
                                DeletedRecords = Command2.ExecuteNonQuery();
                                if (DeletedRecords > 0)
                                {
                                    //Delete From Details Table
                                    Command2.CommandText = "Delete From BookDeliveryRequestDetails Where DeliveryNo = @DeliveryNo";
                                    Command2.Parameters.AddWithValue("@DeliveryNo", DeliveryNo);
                                    DeletedRecords = Command2.ExecuteNonQuery();
                                }
                            }
                            if (DeletedRecords > 0)
                            {
                                Session["dMsg"] = "تم الحفظ بنجاح";
                            }
                            else
                            {
                                Session["dMsg"] = "خطأ أثناء الحفظ";
                            }
                        }

                    }
                    
                }
  
            }
            return RedirectToAction("AllBookDeliveryRequests",Session["dMsg"]);
        }
    }
}