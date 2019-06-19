using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ECBNewWeb.Models;
using ECBNewWeb.DataAccess;
using System.Net;

namespace ECBNewWeb.Controllers
{
    public class BookResposibilitiesController : Controller
    {
        // GET: BookResposibilities
        public ActionResult AddBookResponsibility()
        {
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
                    TempData["Msg"] = "تم الحفظ بنجاح";
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
                                                 join e in db.Employees on b.EmployeeId equals e.EmployeeId
                                                 join h in db.HandleBookReceipts on b.HandleBookReceiptId equals h.BookReceiptId
                                                 join t in db.BookTypes on h.BookTypeId equals t.BookTypeId
                                                 join Re in db.marketingrectypes on t.RecTypeId equals Re.id
                                                 select new BookResposibilityModel()
                                                 {
                                                     RespId = b.RespId,
                                                     FullName = e.FirstName,
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
            ViewBag.myEmployee = emp();
            
            ViewBag.myRecTypes = PopulateRecTypes();

            BookResposibilityModel bookresposibility = new BookResposibilityModel();
            MarketEntities db = new MarketEntities();
            bookresposibility = (from b in db.BookResposibilities
                                                 join e in db.Employees on b.EmployeeId equals e.EmployeeId
                                                 join h in db.HandleBookReceipts on b.HandleBookReceiptId equals h.BookReceiptId
                                                 join t in db.BookTypes on h.BookTypeId equals t.BookTypeId
                                                 join Re in db.marketingrectypes on t.RecTypeId equals Re.id
                                                 where b.RespId == id
                                                 select new BookResposibilityModel()
                                                 {
                                                     RespId = b.RespId,
                                                     FullName = e.FirstName,
                                                     EmployeeId = e.EmployeeId,
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

            ViewBag.myhandle = PopulateHandlebook(bookresposibility.RecTypeId);
            return PartialView(bookresposibility);
        }
        [HttpPost]
        public ActionResult EditBookResponsibility(BookResposibilityModel bookModel)
        {

            if (bookModel.RespId == 0)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var bookRespToUpdate = new BookResposibility();
            using (MarketEntities db = new MarketEntities())
            {
                
                if (ModelState.IsValid)
                {
                    bookRespToUpdate = db.BookResposibilities.Find(bookModel.RespId);
                    bookRespToUpdate.HandleBookReceiptId = bookModel.HandleBookReceiptId;
                    bookRespToUpdate.EmployeeId = bookModel.EmployeeId;
                    bookRespToUpdate.ReceiveDate = bookModel.ReceiveDate;
                    bookRespToUpdate.DeliveryDate = bookModel.DeliveryDate;
                    bookRespToUpdate.DoneFlag =  bookModel.DeliveryDate == null ? 0 : 1;
                    bookRespToUpdate.NextReceiptNo = bookModel.NextReceiptNo;
                    
                    TryUpdateModel(bookRespToUpdate);
                    db.SaveChanges();
                    TempData["Msg"] = "تم الحفظ بنجاح";
                }
                else
                {
                    TempData["Msg"] = "لم يتم الحفظ";
                }
            }
            return RedirectToAction("AllBookResponsibility" , TempData["Msg"]);
        }

        public List<SelectListItem> emp()
        {
            List<SelectListItem> Items = new List<SelectListItem>();

            using (MarketEntities db = new MarketEntities())
            {
                List<EmployeeModel> MyEmployee = (from e in db.Employees
                                             select new EmployeeModel() { EmployeeId = e.EmployeeId , FullName = e.FirstName + " " + e.LastName  }).ToList<EmployeeModel>();
                SelectListItem DisabledList = new SelectListItem()
                {
                    Text = " ",
                    Value = " -1",
                    Selected = true
                };
                Items.Add(DisabledList);
                foreach (EmployeeModel employee in MyEmployee)
                {
                    SelectListItem selectList = new SelectListItem()
                    {
                        Text = employee.FullName,
                        Value = employee.EmployeeId.ToString(),
                    };
                    Items.Add(selectList);
                }
            }
            return Items;
        }

        public List<SelectListItem> PopulateRecTypes()
        {
            List<SelectListItem> Items = new List<SelectListItem>();
            using (MarketEntities db = new MarketEntities())
            {
                List<BookModel> MyRecType = (from Rec in db.marketingrectypes
                                             select new BookModel() { RecTypeId = Rec.id, RecTypeName = Rec.name }).ToList<BookModel>();
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

        public JsonResult PopulateBooks(int bookid)
        {
            List<SelectListItem> Items = new List<SelectListItem>();
            using (MarketEntities db = new MarketEntities())
            {
                List<BookModel> MyHandleBookReceiptId = (from H in db.HandleBookReceipts
                                            join T in db.BookTypes
                                            on H.BookTypeId equals T.BookTypeId
                                            where T.RecTypeId == bookid
                                            select new BookModel() {  BookReceiptId = H.BookReceiptId , BookNo = T.BookNo }).ToList<BookModel>();

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








    }
}