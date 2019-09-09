using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ECBNewWeb.DataAccess;
using ECBNewWeb.Models;
using System.Data;
using System.Data.SqlClient;
using ECBNewWeb.CustomAuthentication;
using System.Web.Security;

namespace ECBNewWeb.Controllers
{
    public class BookRequestsController : Controller
    {
        private CustomMembershipUser UserInfo;
        public BookRequestsController()
        {
           
        }
        // GET: BookRequests
        [HttpGet]
        public ActionResult AddBookRequest()
        {
            if (User.Identity.IsAuthenticated)
            {
                UserInfo = (CustomMembershipUser)Membership.GetUser(HttpContext.User.Identity.Name, false);
                Session["CurrentUser"] = UserInfo.FirstName + " " + UserInfo.MiddleName + " " + UserInfo.LastName;
            }
            BookRequestDetailModel model = new BookRequestDetailModel();
            ViewBag.Bookrequest = PopulateAllBookRequest().FirstOrDefault();
            ViewBag.AllBookRequest = PopulateAllBookRequest();
            ViewBag.EnableBookRequest = PopulateEnableBookRequest().FirstOrDefault();
            ViewBag.EnableEditBookRequest = PopulateEnableEditBookRequest().FirstOrDefault();
            ViewBag.EditBookRequest = PopulateEnableEditBookRequest();
            ViewBag.emplist = emp();
            ViewBag.rectypelist = PopulateRecTypes();
            ViewBag.AllbookReceiveList = BookReceived();
            return View(model);
        }
        [HttpPost]
        public ActionResult AddBookRequest( int[] arr_Amount, int[] receiptTypeId)
        {
            UserInfo = (CustomMembershipUser)Membership.GetUser(HttpContext.User.Identity.Name, false);
            int rowAffected = 0;
            MarketEntities market = new MarketEntities();
            if (ModelState.IsValid)
            {
                var bookrequestmodel = new BookRequest()
                {
                    RequestNo = market.Database.SqlQuery<Int64>("SELECT NEXT VALUE FOR [dbo].[SequenceRequestNo]").FirstOrDefault(),
                    EmployeeId = UserInfo.EmployeeId,
                    RequestDate = DateTime.Now,
                    TimeStamp = DateTime.Now,
                    Active = 1
                };
                market.BookRequests.Add(bookrequestmodel);
                rowAffected = market.SaveChanges();
                for (int i = 0; i < arr_Amount.Length; i++)
                {
                    var bookrequestdetailsmodel = new BookRequestDetail()
                    {
                        RequestNo = bookrequestmodel.RequestNo,
                        ReceiptTypeId = receiptTypeId[i],
                        Amount = arr_Amount[i],
                        SupervisorApproval = 0,
                        FinanceApproval = 0,
                        EmployeeReceive = 0
                    };
                    market.BookRequestDetails.Add(bookrequestdetailsmodel);
                    rowAffected = market.SaveChanges();
                    TempData["Msg"] = rowAffected > 0 ? "تم الحفظ بنجاح" : "لم يتم الحفظ";
                }
            }
            else
            {
                TempData["Msg"] = "لم يتم الحفظ";
            }
            return RedirectToAction("AddBookRequest");
        }
        [HttpGet]
        public ActionResult EditBookrequest(int Id)
        {
            BookRequestDetailModel model = PopulateBookRequestDetailsById(Id);
            ViewBag.BookCountAvailableEdit = BookCountAvailableEdit(model.ReceiptTypeId.Value);
            ViewBag.rectypelist = PopulateRecTypes();
            return PartialView(model);
        }

        [HttpPost]
        public ActionResult EditBookrequest(int Amount, int Id)
        {
            using (MarketEntities db = new MarketEntities())
            {
                if (ModelState.IsValid)
                {
                    var BookRequestDetailToUpdate = db.BookRequestDetails.Where(x => x.Id == Id).FirstOrDefault();

                    BookRequestDetailToUpdate.Amount = Amount;
                    TryUpdateModel(BookRequestDetailToUpdate);
                    int rowAffected = db.SaveChanges();
                    TempData["Msg"] = rowAffected > 0 ? "تم الحفظ بنجاح" : "لم يتم الحفظ";

                }
                else
                {
                    TempData["Msg"] = "لم يتم الحفظ";
                }
            }
            return RedirectToAction("AddBookRequest", TempData["Msg"]);
        }

        [HttpGet]
        public ActionResult DeleteBookRequest(int Id)
        {
            BookRequestDetailModel model = PopulateBookRequestDetailsById(Id);
            ViewBag.rectypelist = PopulateRecTypes();
            return PartialView(model);
        }

        [HttpPost]
        public ActionResult DeleteBookRequestPost(int Id, int RequestNo)
        {
            int rowAffected = 0;
            if (ModelState.IsValid)
            {
                using (MarketEntities db = new MarketEntities())
                {
                    var modelBookRequestDetails = (db.BookRequestDetails.Single(a => a.Id == Id));
                    db.Entry(modelBookRequestDetails).State = EntityState.Deleted;
                    rowAffected = db.SaveChanges();
                    var countRequests = db.BookRequestDetails.Where(x => x.RequestNo == RequestNo).Count();
                    if (countRequests == 0)
                    {
                        var modelBookrequest = (db.BookRequests.Single(a => a.RequestNo == RequestNo));
                        db.Entry(modelBookrequest).State = EntityState.Deleted;
                        rowAffected = db.SaveChanges();
                    }
                    TempData["Msg"] = rowAffected > 0 ? "تم الحفظ بنجاح" : "لم يتم الحفظ";
                }
            }
            else
            {
                TempData["Msg"] = "لم يتم الحفظ";
            }
            return RedirectToAction("AddBookRequest", TempData["Msg"]);
        }

        [HttpGet]
        public ActionResult AddNewTypeBookRequest(int Id)
        {
            BookRequestDetailModel model = new BookRequestDetailModel();
            int RequestNo = Id;
            model.RequestNo = RequestNo;
            ViewBag.rectypelist = PopulateRecTypesNotInRequest(RequestNo);
            return PartialView(model);
        }

        [HttpPost]
        public ActionResult AddNewTypeBookRequest(int RequestNo, int ReceiptTypeId, int Amount)
        {
            int rowAffected = 0;
            if (ModelState.IsValid)
            {
                using (MarketEntities db = new MarketEntities())
                {
                    BookRequestDetail model = new BookRequestDetail();
                    model.RequestNo = RequestNo;
                    model.ReceiptTypeId = ReceiptTypeId;
                    model.Amount = Amount;
                    model.SupervisorApproval = 0;
                    model.FinanceApproval = 0;
                    model.EmployeeReceive = 0;
                    db.BookRequestDetails.Add(model);
                    rowAffected = db.SaveChanges();
                    TempData["Msg"] = rowAffected > 0 ? "تم الحفظ بنجاح" : "لم يتم الحفظ";
                }
            }
            else
            {
                TempData["Msg"] = "لم يتم الحفظ";
            }

            return RedirectToAction("AddBookRequest", TempData["Msg"]);
        }

        public List<BookRequestDetailModel> PopulateAllBookRequest()
        {
            var ListOfBooks = new List<BookRequestDetailModel>();
            using (MarketEntities db = new MarketEntities())
            {
                ListOfBooks = (from br in db.BookRequests
                               join brd in db.BookRequestDetails on br.RequestNo equals brd.RequestNo
                               join t in db.marketingrectypes on brd.ReceiptTypeId equals t.id
                               where br.EmployeeId == UserInfo.EmployeeId //for test
                              // && brd.FinanceApproval == 0 && brd.SupervisorApproval == 0
                               select new BookRequestDetailModel()
                               {
                                   Id = brd.Id,
                                   RequestNo = br.RequestNo,
                                   EmployeeId = br.EmployeeId,
                                   ReceiptTypeId = brd.ReceiptTypeId,
                                   bookTypeName = t.name,
                                   Amount = brd.Amount,
                                   RequestDate = br.RequestDate,
                                   SupervisorApproval = brd.SupervisorApproval,
                                   FinanceApproval = brd.FinanceApproval
                               }).ToList<BookRequestDetailModel>();
            }

            return ListOfBooks;
        }

        public List<BookRequestDetailModel> PopulateEnableBookRequest()
        {
            var ListOfBooks = new List<BookRequestDetailModel>();
            using (MarketEntities db = new MarketEntities())
            {
                ListOfBooks = (from br in db.BookRequests
                               join brd in db.BookRequestDetails on br.RequestNo equals brd.RequestNo
                               join t in db.marketingrectypes on brd.ReceiptTypeId equals t.id
                               where br.EmployeeId == UserInfo.EmployeeId //for test
                               && brd.FinanceApproval == 0 
                               select new BookRequestDetailModel()
                               {
                                   Id = brd.Id,
                                   RequestNo = br.RequestNo,
                                   EmployeeId = br.EmployeeId,
                                   ReceiptTypeId = brd.ReceiptTypeId,
                                   bookTypeName = t.name,
                                   Amount = brd.Amount,
                                   RequestDate = br.RequestDate,
                                   SupervisorApproval = brd.SupervisorApproval,
                                   FinanceApproval = brd.FinanceApproval
                               }).ToList<BookRequestDetailModel>();
            }

            return ListOfBooks;
        }

        public List<BookRequestDetailModel> PopulateEnableEditBookRequest()
        {
            var ListOfBooks = new List<BookRequestDetailModel>();
            using (MarketEntities db = new MarketEntities())
            {
                ListOfBooks = (from br in db.BookRequests
                               join brd in db.BookRequestDetails on br.RequestNo equals brd.RequestNo
                               join t in db.marketingrectypes on brd.ReceiptTypeId equals t.id
                               where br.EmployeeId == UserInfo.EmployeeId //for test
                               && brd.FinanceApproval == 0 && brd.SupervisorApproval == 0
                               select new BookRequestDetailModel()
                               {
                                   Id = brd.Id,
                                   RequestNo = br.RequestNo,
                                   EmployeeId = br.EmployeeId,
                                   ReceiptTypeId = brd.ReceiptTypeId,
                                   bookTypeName = t.name,
                                   Amount = brd.Amount,
                                   RequestDate = br.RequestDate,
                                   SupervisorApproval = brd.SupervisorApproval,
                                   FinanceApproval = brd.FinanceApproval
                               }).ToList<BookRequestDetailModel>();
            }

            return ListOfBooks;
        }
        public BookRequestDetailModel PopulateBookRequestDetailsById(int id)
        {
            var BookDetailsById = new BookRequestDetailModel();
            using (  MarketEntities db = new MarketEntities())
            {
                BookDetailsById = (from brd in db.BookRequestDetails
                                   join t in db.marketingrectypes on brd.ReceiptTypeId equals t.id
                                   where brd.Id == id
                                   select new BookRequestDetailModel()
                                   {
                                       Id = brd.Id,
                                       ReceiptTypeId = brd.ReceiptTypeId,
                                       bookTypeName = t.name,
                                       Amount = brd.Amount,
                                       RequestNo = brd.RequestNo
                                   }).FirstOrDefault();
            }
            return BookDetailsById;
        }

        public List<SelectListItem> emp()
        {
            UserInfo = (CustomMembershipUser)Membership.GetUser(HttpContext.User.Identity.Name, false);
            List<SelectListItem> Items = new List<SelectListItem>();
            using (MarketEntities db = new MarketEntities())
            {
                List<LoginViewModel> MyEmployee = (from e in db.UserLogins
                                                   join m in db.Employees
                                                   on e.employee_id equals m.EmployeeId
                                                   where e.active == 1 && m.Active == 1
                                                   select new LoginViewModel() { UserId = m.EmployeeId, FullName = m.FirstName + " " + m.MiddleName + " " + m.LastName }).ToList<LoginViewModel>();
                SelectListItem DisabledList = new SelectListItem()
                {
                    Text = " ",
                    Value = " -1",
                    Selected = true
                };
                Items.Add(DisabledList);
                foreach (LoginViewModel employee in MyEmployee)
                {
                    SelectListItem selectList = new SelectListItem()
                    {
                        Text = employee.FullName,
                        Value = employee.UserId.ToString(),
                    };
                    Items.Add(selectList);
                }
            }
            return Items;
        }

        public List<SelectListItem> PopulateRecTypes()
        {
            UserInfo = (CustomMembershipUser)Membership.GetUser(HttpContext.User.Identity.Name, false);
            List<SelectListItem> Items = new List<SelectListItem>();
            using (MarketEntities db = new MarketEntities())
            {
                UserInfo = (CustomMembershipUser)Membership.GetUser(HttpContext.User.Identity.Name, false);
                List<marketingrectypeModel> MyRecType = db.Database.SqlQuery<marketingrectypeModel>("select distinct p.id , p.name , ISNULL(tt.su , 0) as rec from marketingrectype p inner join BookTypes t on p.id = t.RecTypeId left join (select distinct rec.id , rec.name , count(t.RecTypeId) as su from marketingrectype rec left join BookTypes t on rec.id = t.RecTypeId left join HandleBookReceipts h on t.BookTypeId = h.BookTypeId left join BookResposibilities r on h.BookReceiptId = r.HandleBookReceiptId where r.EmployeeId = @empid and DeliveryDate is null group by rec.id , rec.name) tt on p.id = tt.id where tt.su is null or tt.su != 3 ", new SqlParameter("@empid", UserInfo.EmployeeId)).ToList();
                SelectListItem DisabledList = new SelectListItem()
                {
                    Text = " ",
                    Value = " -1",
                    Selected = true
                };
                Items.Add(DisabledList);
                foreach (marketingrectypeModel Rec in MyRecType)
                {

                    SelectListItem selectList = new SelectListItem()
                    {
                        Text = Rec.name,
                        Value = Rec.id.ToString(),
                    };

                    Items.Add(selectList);

                }
            }
            return Items;
        }

        public List<SelectListItem> PopulateRecTypesNotInRequest(int requestNo)
        {
            UserInfo = (CustomMembershipUser)Membership.GetUser(HttpContext.User.Identity.Name, false);
            List<SelectListItem> Items = new List<SelectListItem>();
            using (MarketEntities db = new MarketEntities())
            {

                List<marketingrectypeModel> MyRecType = db.Database.SqlQuery<marketingrectypeModel>("select distinct p.id , p.name , ISNULL(tt.su , 0) as recTypeCount from marketingrectype p inner join BookTypes t on p.id = t.RecTypeId left join (select distinct rec.id , rec.name , count(t.RecTypeId) as su from marketingrectype rec left join BookTypes t on rec.id = t.RecTypeId left join HandleBookReceipts h on t.BookTypeId = h.BookTypeId left join BookResposibilities r on h.BookReceiptId = r.HandleBookReceiptId where r.EmployeeId = @empid and DeliveryDate is null group by rec.id , rec.name) tt on p.id = tt.id where (tt.su is null or tt.su != 3) and p.id not in (select ReceiptTypeId from BookRequestDetails where RequestNo = @requestNo)", new SqlParameter("@requestNo", requestNo), new SqlParameter("@empid", UserInfo.EmployeeId)).ToList();
                SelectListItem DisabledList = new SelectListItem()
                {
                    Text = " ",
                    Value = " -1",
                    Selected = true
                };
                Items.Add(DisabledList);
                foreach (marketingrectypeModel Rec in MyRecType)
                {

                    SelectListItem selectList = new SelectListItem()
                    {
                        Text = Rec.name,
                        Value = Rec.id.ToString(),
                    };

                    Items.Add(selectList);

                }
            }
            return Items;

        }

        public JsonResult CountBookbyType(int Rectypeid, int employeeid)
        {
            UserInfo = (CustomMembershipUser)Membership.GetUser(HttpContext.User.Identity.Name, false);
            MarketEntities db = new MarketEntities();
            var CountBookbyEmployee = (from b in db.BookResposibilities
                                       join h in db.HandleBookReceipts on b.HandleBookReceiptId equals h.BookReceiptId
                                       join t in db.BookTypes on h.BookTypeId equals t.BookTypeId
                                       where t.RecTypeId == Rectypeid && b.EmployeeId == UserInfo.EmployeeId && b.DeliveryDate == null
                                       select new BookResposibilityModel()
                                       {
                                           RecTypeId = t.RecTypeId,
                                           EmployeeId = b.EmployeeId
                                       }).Count();

            return Json(CountBookbyEmployee, JsonRequestBehavior.AllowGet);
        }

        // Get BookType Count Available to This Employee
        public JsonResult BookCountAvailable(int requestNo)
        {
            UserInfo = (CustomMembershipUser)Membership.GetUser(HttpContext.User.Identity.Name, false);
            MarketEntities market = new MarketEntities();
            var m = market.Database.SqlQuery<marketingrectypeModel>("select distinct p.id , p.name , ISNULL((3 -tt.su) , 3) as recTypeCount from marketingrectype p inner join BookTypes t on p.id = t.RecTypeId left join (select distinct rec.id , rec.name , count(t.RecTypeId) as su from marketingrectype rec left join BookTypes t on rec.id = t.RecTypeId left join HandleBookReceipts h on t.BookTypeId = h.BookTypeId left join BookResposibilities r on h.BookReceiptId = r.HandleBookReceiptId where r.EmployeeId = @Empid  and DeliveryDate is null group by rec.id , rec.name ) tt on p.id = tt.id where (tt.su is null or tt.su != 3) and p.id not in (select ReceiptTypeId from BookRequestDetails where RequestNo = @requestNo)", new SqlParameter("@requestNo ", 23), new SqlParameter("@Empid ", UserInfo.EmployeeId));
            var BookCountAvailable = m.ToList();
            return Json(BookCountAvailable, JsonRequestBehavior.AllowGet);
        }

        public JsonResult BookCountAvailableFirstAdd()
        {
            UserInfo = (CustomMembershipUser)Membership.GetUser(HttpContext.User.Identity.Name, false);
            MarketEntities market = new MarketEntities();
            var m = market.Database.SqlQuery<marketingrectypeModel>("select distinct p.id , p.name , ISNULL((3 -tt.su) , 3) as recTypeCount from marketingrectype p inner join BookTypes t on p.id = t.RecTypeId left join (select distinct rec.id , rec.name , count(t.RecTypeId) as su from marketingrectype rec left join BookTypes t on rec.id = t.RecTypeId left join HandleBookReceipts h on t.BookTypeId = h.BookTypeId left join BookResposibilities r on h.BookReceiptId = r.HandleBookReceiptId where r.EmployeeId = @Empid  and DeliveryDate is null  group by rec.id , rec.name ) tt on p.id = tt.id where (tt.su is null or tt.su != 3) ", new SqlParameter("@Empid ", UserInfo.EmployeeId));
            var BookCountAvailable = m.ToList();
            return Json(BookCountAvailable, JsonRequestBehavior.AllowGet);
        }

        public List<marketingrectypeModel> BookReceived()
        {
            UserInfo = (CustomMembershipUser)Membership.GetUser(HttpContext.User.Identity.Name, false);
            MarketEntities market = new MarketEntities();
            var m = market.Database.SqlQuery<marketingrectypeModel>("select distinct p.id , p.name , ISNULL((tt.su) , 0) as recTypeCount from marketingrectype p inner join BookTypes t on p.id = t.RecTypeId left join (select distinct rec.id , rec.name , count(t.RecTypeId) as su from marketingrectype rec left join BookTypes t on rec.id = t.RecTypeId left join HandleBookReceipts h on t.BookTypeId = h.BookTypeId left join BookResposibilities r on h.BookReceiptId = r.HandleBookReceiptId where r.EmployeeId = @Empid  and r.DeliveryDate is null  group by rec.id , rec.name ) tt on p.id = tt.id order by recTypeCount desc ", new SqlParameter("@Empid ", UserInfo.EmployeeId));
            var BookReceived = m.ToList();
            return BookReceived;
        }

        public int BookCountAvailableEdit(int rectypeID)
        {
            UserInfo = (CustomMembershipUser)Membership.GetUser(HttpContext.User.Identity.Name, false);
            MarketEntities market = new MarketEntities();
            var m = market.Database.SqlQuery<marketingrectypeModel>("select  ISNULL((3 -tt.su) , 3) as recTypeCount from marketingrectype p inner join BookTypes t on p.id = t.RecTypeId left join (select distinct rec.id , rec.name , count(t.RecTypeId) as su from marketingrectype rec left join BookTypes t on rec.id = t.RecTypeId left join HandleBookReceipts h on t.BookTypeId = h.BookTypeId left join BookResposibilities r on h.BookReceiptId = r.HandleBookReceiptId where r.EmployeeId = @Empid  and and DeliveryDate is null  group by rec.id , rec.name ) tt on p.id = tt.id where (tt.su is null or tt.su != 3) and p.id = @recTypeId ", new SqlParameter("@Empid ", UserInfo.EmployeeId), new SqlParameter("@recTypeId ", rectypeID)).FirstOrDefault();
            int result = m.recTypeCount;
            return result;
        }

    }

  
}