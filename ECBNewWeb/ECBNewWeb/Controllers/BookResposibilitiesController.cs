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

namespace ECBNewWeb.Controllers
{
    public class BookResposibilitiesController : Controller
    {
        private CustomMembershipUser UserInfo;
        // GET: BookResposibilities
        public ActionResult AddBookResponsibility()
        {
            if (User.Identity.IsAuthenticated)
            {
                UserInfo = (CustomMembershipUser)Membership.GetUser(HttpContext.User.Identity.Name, false);
                Session["CurrentUser"] = UserInfo.FirstName + " " + UserInfo.MiddleName + " " + UserInfo.LastName;
            }
            BookResposibilityModel _BookResposibilityModel = new BookResposibilityModel();
            _BookResposibilityModel.MyEmployee = emp();
            _BookResposibilityModel.MyRecTypes = PopulateRecTypes();
            return View(_BookResposibilityModel);
        }

        [HttpPost]
        public ActionResult AddBookResponsibility(BookResposibilityModel bookresponsemodel)
        {

            using (MarketEntities Market = new MarketEntities())
            {
                if (ModelState.IsValid)
                {
                    BookResposibility dbBookResopsibility = new BookResposibility();
                    dbBookResopsibility.HandleBookReceiptId = bookresponsemodel.HandleBookReceiptId;
                    dbBookResopsibility.EmployeeId = bookresponsemodel.EmployeeId;
                    dbBookResopsibility.ReceiveDate = bookresponsemodel.ReceiveDate;
                    dbBookResopsibility.DeliveryDate = bookresponsemodel.DeliveryDate;
                    dbBookResopsibility.NextReceiptNo = bookresponsemodel.NextReceiptNo;
                    dbBookResopsibility.DoneFlag = bookresponsemodel.DoneFlag == 1? 1 : 0;
                    Market.BookResposibilities.Add(dbBookResopsibility);
                    int rowAffected = Market.SaveChanges();
                    TempData["Msg"] = rowAffected > 0 ? "تم الحفظ بنجاح" : "لم يتم الحفظ";
                }
                else
                {
                    TempData["Msg"] = "لم يتم الحفظ";
                }
            }
            return RedirectToAction("AddBookResponsibility");
        }

        public ActionResult AllBookResponsibility()
        {
            MarketEntities db = new MarketEntities() ;
            List<BookResposibilityModel> list = (from b in db.BookResposibilities
                                                 join e in db.UserLogins on b.EmployeeId equals e.id 
                                                 join h in db.HandleBookReceipts on b.HandleBookReceiptId equals h.BookReceiptId
                                                 join t in db.BookTypes on h.BookTypeId equals t.BookTypeId
                                                 join Re in db.marketingrectypes on t.RecTypeId equals Re.id
                                                 select new BookResposibilityModel()
                                                 {
                                                     RespId = b.RespId,
                                                     FirstName = e.FirstName ,
                                                     lastName = e.LastName,
                                                     BookNo = t.BookNo,
                                                     ReceiptTypeName = Re.name,                  
                                                    DeliveryDate = b.DeliveryDate,
                                                    ReceiveDate = b.ReceiveDate,
                                                    NextReceiptNo = b.NextReceiptNo,
                                                    DoneFlag = b.DoneFlag
                                                }).ToList<BookResposibilityModel>();
           
            return View(list);
        }

        [HttpGet]
        public ActionResult EditBookResponsibility(int id)
        {
            if (User.Identity.IsAuthenticated)
            {
                UserInfo = (CustomMembershipUser)Membership.GetUser(HttpContext.User.Identity.Name, false);
                Session["CurrentUser"] = Membership.GetUser(HttpContext.User.Identity.Name, false);
            }
            ViewBag.myEmployee = emp();          
            ViewBag.myRecTypes = PopulateRecTypes();
            BookResposibilityModel bookresposibility = new BookResposibilityModel();
            MarketEntities db = new MarketEntities();
            var getBookResposiblilty =  db.BookResposibilities.Find(id);
            if(getBookResposiblilty.PartialBookIndicator == 1)
            {
                bookresposibility = (from b1 in db.BookResposibilities
                                     join b2 in db.BookResposibilities on b1.EmployeeId equals b2.ParentEmployeeId
                                     join e in db.UserLogins on b1.EmployeeId equals e.id
                                     join h in db.HandleBookReceipts on b1.HandleBookReceiptId equals h.BookReceiptId
                                     join t in db.BookTypes on h.BookTypeId equals t.BookTypeId
                                     join Re in db.marketingrectypes on t.RecTypeId equals Re.id
                                     where b1.PartialBookIndicator == 1 && b1.RespId == id
                                     select new BookResposibilityModel
                                     {
                                         RespId = b1.RespId,
                                         FullName = e.FirstName,
                                         EmployeeId = e.id,
                                         BookNo = t.BookNo,
                                         ReceiptTypeName = Re.name,
                                         DeliveryDate = b1.DeliveryDate,
                                         ReceiveDate = b1.ReceiveDate,
                                         NextReceiptNo = b1.NextReceiptNo,
                                         DoneFlag = b1.DoneFlag,
                                         RecTypeId = t.RecTypeId,
                                         HandleBookReceiptId = b1.HandleBookReceiptId,
                                         BookReceiptId = h.BookReceiptId,
                                         ////////////////////
                                         PartialBookIndicator = b1.PartialBookIndicator,
                                         RespId2 = b2.RespId,
                                         EmployeeId2 = b2.EmployeeId,
                                         ReceiveDate2 = b2.ReceiveDate,
                                         DeliveryDate2 = b2.DeliveryDate,

                                     }).FirstOrDefault();
            } 
            else
            {
                bookresposibility = (from b in db.BookResposibilities
                                     join e in db.UserLogins on b.EmployeeId equals e.id
                                     join h in db.HandleBookReceipts on b.HandleBookReceiptId equals h.BookReceiptId
                                     join t in db.BookTypes on h.BookTypeId equals t.BookTypeId
                                     join Re in db.marketingrectypes on t.RecTypeId equals Re.id
                                     where b.RespId == id
                                     select new BookResposibilityModel()
                                     {
                                         RespId = b.RespId,
                                         FullName = e.FirstName,
                                         EmployeeId = e.id,
                                         BookNo = t.BookNo,
                                         ReceiptTypeName = Re.name,
                                         DeliveryDate = b.DeliveryDate,
                                         ReceiveDate = b.ReceiveDate,
                                         NextReceiptNo = b.NextReceiptNo,
                                         DoneFlag = b.DoneFlag,
                                         RecTypeId = t.RecTypeId,
                                         HandleBookReceiptId = b.HandleBookReceiptId,
                                         BookReceiptId = h.BookReceiptId

                                     }).FirstOrDefault();
            }
            ViewBag.Employee2 = empexecptid(bookresposibility.EmployeeId);
            ViewBag.myhandle = PopulateHandlebook(bookresposibility.RecTypeId);
            return PartialView(bookresposibility);
        }
        [HttpPost]
        public ActionResult EditBookResponsibility(BookResposibilityModel bookModel , bool cb_otheremployee = false  )
        {
            if (bookModel.RespId == 0)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            BookResposibility bookRespToUpdate = new BookResposibility();
            BookResposibility bookRespToUpdate2 = new BookResposibility();
            BookResposibility bookresp2update = new BookResposibility();
            using (MarketEntities db = new MarketEntities())
            { 
                if (ModelState.IsValid)
                {
                    int rowAffected = 0;
                    if(cb_otheremployee == true)
                    {
                        if(bookModel.RespId2 > 0)
                        {
                            //update
                            int respid =  bookModel.RespId2;
                            bookresp2update = db.BookResposibilities.Where(x => x.RespId == respid).FirstOrDefault();
                            bookresp2update.EmployeeId = bookModel.EmployeeId2;
                            bookresp2update.ReceiveDate = bookModel.ReceiveDate2;
                            bookresp2update.DeliveryDate = bookModel.DeliveryDate2;
                            bookresp2update.DoneFlag = bookModel.DeliveryDate2 == null ? 0 : 1;
                            rowAffected = db.SaveChanges();

                        }
                        else
                        {
                            //insert 
                            bookRespToUpdate.ReceiveDate = bookModel.ReceiveDate2;
                            bookRespToUpdate.DeliveryDate = bookModel.DeliveryDate2;
                            bookRespToUpdate.HandleBookReceiptId = bookModel.HandleBookReceiptId;
                            bookRespToUpdate.EmployeeId = bookModel.EmployeeId2;
                            bookRespToUpdate.ParentEmployeeId = bookModel.EmployeeId;
                            bookRespToUpdate.DoneFlag = bookModel.DeliveryDate2 == null ? 0 : 1;
                            bookRespToUpdate.NextReceiptNo = bookModel.NextReceiptNo;
                            db.BookResposibilities.Add(bookRespToUpdate);
                            rowAffected = db.SaveChanges();
                            // update
                            bookRespToUpdate2 = db.BookResposibilities.Find(bookModel.RespId);
                            bookRespToUpdate2.PartialBookIndicator = 1;
                            rowAffected = db.SaveChanges();
                        }
                        

                    }
                    else
                    {
                        //update
                        bookRespToUpdate = db.BookResposibilities.Find(bookModel.RespId);
                        bookRespToUpdate.HandleBookReceiptId = bookModel.HandleBookReceiptId;
                        bookRespToUpdate.EmployeeId = bookModel.EmployeeId;
                        bookRespToUpdate.ReceiveDate = bookModel.ReceiveDate;
                        bookRespToUpdate.DeliveryDate = bookModel.DeliveryDate;
                        bookRespToUpdate.DoneFlag = bookModel.DeliveryDate == null ? 0 : 1;
                        bookRespToUpdate.NextReceiptNo = bookModel.NextReceiptNo;
                        TryUpdateModel(bookRespToUpdate);
                         rowAffected = db.SaveChanges();
                    }
                    TempData["Msg"] = rowAffected > 0 ? "تم الحفظ بنجاح" : "لم يتم الحفظ";
                }
                else
                {
                    TempData["Msg"] = "لم يتم الحفظ";
                }
            }
            return RedirectToAction("AllBookResponsibility" , TempData["Msg"]);
        }
        // get employee
        public List<SelectListItem> emp()
        {
            List<SelectListItem> Items = new List<SelectListItem>();
            using (MarketEntities db = new MarketEntities())
            {
                List<LoginViewModel> MyEmployee = (from e in db.UserLogins
                                                    join m in db.Employees
                                                    on e.employee_id equals m.EmployeeId
                                                    where e.active == 1 && m.Active == 1 && m.ParentEmployeeId == UserInfo.EmployeeId
                                                    select new LoginViewModel() { UserId = e.id, FullName = m.FirstName + " " + m.MiddleName + " " + m.LastName }).ToList<LoginViewModel>();
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


        public List<SelectListItem> empexecptid(int ? id)
        {
            List<SelectListItem> Items = new List<SelectListItem>();
            using (MarketEntities db = new MarketEntities())
            {
                List<LoginViewModel> MyEmployee = (from e in db.UserLogins
                                                   where e.id != id
                                                   select new LoginViewModel() { UserId = e.id, FullName = e.FirstName + " " + e.LastName }).ToList<LoginViewModel>();
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
        // get book type && licences.active = 1 && 
        public List<SelectListItem> PopulateRecTypes()
        {
            List<SelectListItem> Items = new List<SelectListItem>();
            using (MarketEntities db = new MarketEntities())
            {
                List<BookModel> MyRecType = (from Rec in db.marketingrectypes
                                             join b in db.BookTypes on Rec.id equals b.RecTypeId
                                             join l in db.MarketingLicenses on b.LicenseId equals l.Id
                                             where l.Active == 1
                                             select new BookModel() { RecTypeId = Rec.id, RecTypeName = Rec.name }).Distinct().ToList<BookModel>();
                foreach (BookModel Rec in MyRecType)
                {

                    SelectListItem selectList = new SelectListItem()
                    {
                        Text = Rec.RecTypeName,
                        Value = Rec.RecTypeId.ToString(),
                    };

                    Items.Add(selectList);

                }
            }
            return Items;

        }

        //get book number 
        public List<SelectListItem> PopulateHandlebook(int bookid)
        {
            List<SelectListItem> Items = new List<SelectListItem>();
            using (MarketEntities db = new MarketEntities())
            {
                List<BookModel> MyHandleBookReceiptId = (from H in db.HandleBookReceipts
                                                         join T in db.BookTypes
                                                         on H.BookTypeId equals T.BookTypeId
                                                         where T.RecTypeId == bookid
                                                         select new BookModel() { BookReceiptId = H.BookReceiptId, BookNo = T.BookNo }).ToList<BookModel>();

                foreach (BookModel no in MyHandleBookReceiptId)
                {

                    SelectListItem selectList = new SelectListItem()
                    {
                        Text = no.BookNo.ToString(),
                        Value = no.BookReceiptId.ToString()
                    };
                    Items.Add(selectList);
                }
            }
            BookModel mybook = new BookModel()
            {
                MyHandleBookReceipts = Items
            };
            return Items;

        }

        public JsonResult otheremp(int parentemp)
        {
            List<SelectListItem> Items = new List<SelectListItem>();
            using (MarketEntities db = new MarketEntities())
            {
                List<LoginViewModel> MyEmployee = (from e in db.UserLogins
                                                   where e.id != parentemp
                                                   select new LoginViewModel() { UserId = e.id, FullName = e.FirstName + " " + e.LastName }).ToList<LoginViewModel>();
                //SelectListItem DisabledList = new SelectListItem()
                //{
                //    Text = " ",
                //    Value = " -1",
                //    Selected = true
                //};
                //Items.Add(DisabledList);
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
            return Json(Items, JsonRequestBehavior.AllowGet);
        }

        // get number of employee receive book of this type
        public JsonResult CountBookbyType(int Rectypeid, int employeeid)
        {
            MarketEntities db = new MarketEntities() ;
             var   CountBookbyEmployee = (from b in db.BookResposibilities
                                           join h in db.HandleBookReceipts on b.HandleBookReceiptId equals h.BookReceiptId
                                           join t in db.BookTypes on h.BookTypeId equals t.BookTypeId
                                           where t.RecTypeId == Rectypeid && b.EmployeeId == employeeid && b.DoneFlag == 0
                                           select new BookResposibilityModel()
                                           {
                                               RecTypeId = t.RecTypeId,
                                               EmployeeId = b.EmployeeId
                                           }).Count();

                

            return Json(CountBookbyEmployee, JsonRequestBehavior.AllowGet);
        }


        //get book number Json
        public JsonResult PopulateBooks(int bookid , int empid)
        {
            List<SelectListItem> Items = new List<SelectListItem>();
            using (MarketEntities db = new MarketEntities())
            {
                List<BookModel> MyHandleBookReceiptId = (from H in db.HandleBookReceipts
                                            join T in db.BookTypes on H.BookTypeId equals T.BookTypeId
                                            //join R in db.BookResposibilities on H.BookReceiptId equals R.HandleBookReceiptId
                                            where T.RecTypeId == bookid && !(from r in db.BookResposibilities
                                                                             join h in db.HandleBookReceipts on r.HandleBookReceiptId equals h.BookReceiptId
                                                                             join t in db.BookTypes on h.BookReceiptId equals t.BookTypeId
                                                                             where r.EmployeeId == empid && t.RecTypeId == bookid
                                                                             select t.BookNo).Contains(T.BookNo)
                                            select new BookModel() {  BookReceiptId = H.BookReceiptId , BookNo = T.BookNo }).Distinct().ToList<BookModel>();

                foreach (BookModel no in MyHandleBookReceiptId)
                {

                    SelectListItem selectList = new SelectListItem()
                    {
                        Text = no.BookNo.ToString(),
                        Value = no.BookReceiptId.ToString()
                    };
                    Items.Add(selectList);
                }
            }
            BookModel mybook = new BookModel()
            {
                MyHandleBookReceipts = Items
            };
            return Json(mybook.MyHandleBookReceipts, JsonRequestBehavior.AllowGet);
        }

        //  get number of First Receipt Number
        public JsonResult PopulateFirstReceiptNumber(int Handlebookreceiptid)
        {

            using (MarketEntities db = new MarketEntities())
            {
                var FirstReceiptNumber = (from H in db.HandleBookReceipts
                                          where H.BookReceiptId == Handlebookreceiptid
                                          select new BookModel() { FirstReceiptNo = H.FirstReceiptNo }).First();
                BookModel mybook = new BookModel();
                mybook = FirstReceiptNumber;

                return Json(mybook.FirstReceiptNo, JsonRequestBehavior.AllowGet);
            }
        }

        // get number of First Receit Number Bet. Two Employee
        public JsonResult PopulateFirstReceiptNumberEmployeeTwo(int Handlebookreceiptid , int RecType )
        {

            using (MarketEntities db = new MarketEntities())
            {
                var FirstReceiptNumber = (from M in db.markets 
                                          join B in db.BookResposibilities on M.ResponsibilityId equals B.RespId
                                          join H in db.HandleBookReceipts on B.HandleBookReceiptId equals H.BookReceiptId
                                          where M.type == RecType  && B.HandleBookReceiptId == Handlebookreceiptid
                                          select new BookModel() { FirstReceiptNo = (M.no + 1) }).First();
                BookModel mybook = new BookModel();
                mybook = FirstReceiptNumber;

                return Json(mybook.FirstReceiptNo, JsonRequestBehavior.AllowGet);
            }
        }

        // check if another employee receive this Book 
        public JsonResult GetBooknumberAndEmployee(int Rectypeid , int Bookno)
        {
            MarketEntities db = new MarketEntities();
            var CountBookbyEmployeeReceive = (from b in db.BookResposibilities
                                       join h in db.HandleBookReceipts on b.HandleBookReceiptId equals h.BookReceiptId
                                       join t in db.BookTypes on h.BookTypeId equals t.BookTypeId
                                       where t.RecTypeId == Rectypeid  && t.BookNo == Bookno
                                       select new BookResposibilityModel()
                                       {
                                           BookNo = t.BookNo,
                                       }).Count();



            return Json(CountBookbyEmployeeReceive, JsonRequestBehavior.AllowGet);
        }










    }
}