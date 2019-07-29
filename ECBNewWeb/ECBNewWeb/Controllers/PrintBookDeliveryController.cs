﻿using CrystalDecisions.CrystalReports.Engine;
using CrystalDecisions.Shared;
using ECBNewWeb.CustomAuthentication;
using ECBNewWeb.DataAccess;
using ECBNewWeb.Models;
using Newtonsoft.Json;
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
    public class PrintBookDeliveryController : Controller
    {
        private CustomMembershipUser UserInfo;
        // GET: PrintBookDelivery
        public ActionResult PrintDeliveryRequests()
        {
            if (User.Identity.IsAuthenticated)
            {
                UserInfo = (CustomMembershipUser)Membership.GetUser(HttpContext.User.Identity.Name, false);
                Session["CurrentUser"] = UserInfo.FirstName + " " + UserInfo.MiddleName + " " + UserInfo.LastName;
            }
            BookDeliveryModel _Model = new BookDeliveryModel();
            _Model.MyDeliveryNo = PopulateRequests();
            return View(_Model);
        }
        private List<SelectListItem> PopulateRequests()
        {
            List<SelectListItem> Items = new List<SelectListItem>();
            using (MarketEntities db = new MarketEntities())
            {
                List<BookDeliveryModel> MyRequests = (from S in db.BookDeliveryRequests
                                                      join D in db.BookRequests on S.RequestId equals D.RequestId
                                                      join J in db.BookDeliveryRequestDetails on S.DeliveryNo equals J.DeliveryNo
                                                      join E in db.Employees on S.EmployeeId equals E.EmployeeId
                                                      where E.ParentEmployeeId == UserInfo.EmployeeId &&
                                                      J.SupervisorApproval == 1 && J.FinanceApproval == 1
                                                      select new BookDeliveryModel() { RequestId = S.RequestId, DeliveryNo = S.DeliveryNo, EmployeeNo = E.EmployeeNo }).OrderByDescending(order => order.DeliveryNo).Distinct().ToList<BookDeliveryModel>();
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
        public JsonResult GetBookDeliveryRequests(int RequestId)
        {
            UserInfo = (CustomMembershipUser)Membership.GetUser(HttpContext.User.Identity.Name, false);
            DataTable dt = new DataTable();
            SqlDataAdapter adapt = null;
            string JsonString = null;
            string Cmd = "Select BookRequests.RequestNo,BookDeliveryRequest.DeliveryId,BookDeliveryRequest.DeliveryNo,Employees.EmployeeNo,Employees.FirstName+' '+Employees.MiddleName+' '+Employees.LastName as FullEmpName, " +
                          "Departments.DepartmentName,BookDeliveryRequest.DeliveryDate,marketingrectype.[name],Count(distinct BookResposibilities.RespId) BookCount, " +
                          "Min(BookTypes.BookNo)FromBook,Max(BookTypes.BookNo)ToBook,Min(HandleBookReceipts.FirstReceiptNo) as FromReceipt,Max(HandleBookReceipts.LastReceiptNo) as ToReceipt " +
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
                          "Where BookResposibilities.DoneFlag = 0 " +
                          "And BookDeliveryRequest.RequestId = @RequestId " +
                          "Group By marketingrectype.[name],BookDeliveryRequest.DeliveryId,Employees.EmployeeNo,Employees.FirstName,Employees.MiddleName,Employees.LastName,Departments.DepartmentName, " +
                          "BookDeliveryRequest.DeliveryNo,BookDeliveryRequest.DeliveryDate,BookRequests.RequestNo " +
                          "Order By marketingrectype.[name]";
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
        public ActionResult PrintReport(BookDeliveryModel model)
        {
            string Cmd = "Select BookRequests.RequestNo,BookDeliveryRequest.DeliveryId,BookDeliveryRequest.DeliveryNo,Employees.EmployeeNo,Employees.FirstName+' '+Employees.MiddleName+' '+Employees.LastName as FullEmpName, " +
                          "Departments.DepartmentName,BookDeliveryRequest.DeliveryDate,BookRequests.RequestDate,marketingrectype.[name],Count(distinct BookResposibilities.RespId) BookCount, " +
                          "Min(BookTypes.BookNo)FromBook,Max(BookTypes.BookNo)ToBook,Min(HandleBookReceipts.FirstReceiptNo) as FromReceipt,Max(HandleBookReceipts.LastReceiptNo) as ToReceipt " +
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
                          "Where BookResposibilities.DoneFlag = 0 " +
                          "And BookDeliveryRequest.RequestId = @RequestId " +
                          "Group By marketingrectype.[name],BookDeliveryRequest.DeliveryId,Employees.EmployeeNo,Employees.FirstName,Employees.MiddleName,Employees.LastName,Departments.DepartmentName, " +
                          "BookDeliveryRequest.DeliveryNo,BookDeliveryRequest.DeliveryDate,BookRequests.RequestDate,BookRequests.RequestNo " +
                          "Order By marketingrectype.[name]";
            using (SqlConnection Conn = new SqlConnection(ConfigurationManager.ConnectionStrings["ECBConnectionString"].ConnectionString))
            {
                Conn.Open();
                using (SqlCommand Com = new SqlCommand(Cmd, Conn))
                {
                    Com.Parameters.AddWithValue("@RequestId", model.RequestId);
                    SqlDataAdapter Adapt = new SqlDataAdapter(Com);
                    DataTable dt = new DataTable();
                    Adapt.Fill(dt);
                    ReportDocument Doc = new ReportDocument();
                    Doc.Load(Path.Combine(Server.MapPath("~/CrystalReports/Marketing"), "rptMarketingBookDelivery.rpt"));
                    Doc.SetDataSource(dt);
                    if (dt.Rows.Count > 0)
                    {
                        try
                        {
                            Stream stream = Doc.ExportToStream(ExportFormatType.PortableDocFormat);
                            stream.Seek(0, SeekOrigin.Begin);
                            return File(stream, "application/pdf");
                        }
                        catch (Exception ex)
                        {

                        }
                    }
                }
            }
            return RedirectToAction("PrintRequests");
        }
    }
}