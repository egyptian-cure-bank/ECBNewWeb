using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ECBNewWeb.DataAccess;
using ECBNewWeb.Models;
using System.Net;

namespace ECBNewWeb.Controllers
{
    public class BookController : Controller
    {
        int GLobalLicenseId = 0;
        string GlobalLicName = string.Empty;

        public BookController()
        {
            //get Current license
            using (MarketEntities db = new MarketEntities())
            {
                var CurrentDate = DateTime.Now.Date;
                var LicData = (from lic in db.MarketingLicenses
                             where lic.Active == 1 &&
                             (CurrentDate >= lic.FromDate && CurrentDate <= lic.ToDate)
                             select new { lic.Id ,lic.LicenseName}).FirstOrDefault();
                GLobalLicenseId = (Int32)LicData.Id;
                GlobalLicName = LicData.LicenseName;
            }
        }
        // GET: Book
        public ActionResult AddBook()
        {
            BookModel _BookModel = new BookModel();
            _BookModel.MyRecTypes = PopulateRecTypes();
            ViewBag.CurrentLicenseName = GlobalLicName;
            return View(_BookModel);
        }
        public List<SelectListItem> PopulateRecTypes()
        {
            List<SelectListItem> Items = new List<SelectListItem>();
            using (MarketEntities db = new MarketEntities())
            {
                List<BookModel> MyRecType = (from Rec in db.marketingrectypes
                                         select new BookModel() { RecTypeId = Rec.id, RecTypeName = Rec.name}).ToList<BookModel>();
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
        [HttpPost]
        public ActionResult SaveBook(int RecTypeId, int NumberofReceipts , int NumberOfBookRequests)
        {
            int rowAffected = 0;
            if (ModelState.IsValid)
            {
                using (MarketEntities Market = new MarketEntities())
                {
                    // LB  = last Book number of type (active)
                    var LB = (from b in Market.BookTypes
                              where b.RecTypeId == RecTypeId && b.LicenseId == 1
                              orderby b.BookNo descending
                              select  b.BookNo ).FirstOrDefault();
                    // if (LB > 0 ) LB = LB + 1 else(LB = 0 Or Null) LB = 1;
                    LB = LB > 0 ? LB + 1 : 1;
                    for (int i = 1; i <= NumberOfBookRequests; i++)
                    {
                        var BookTypeSave = new BookType()
                        {
                            BookNo = LB,
                            RecTypeId = RecTypeId,
                            LicenseId = GLobalLicenseId,
                            Active = 1
                        };
                         Market.BookTypes.Add(BookTypeSave);
                        rowAffected = Market.SaveChanges();
                        //save values to handlebookreceipts table
                        var HBookReceiptSave = new HandleBookReceipt()
                        {
                            BookTypeId = BookTypeSave.BookTypeId,
                            FirstReceiptNo = ((LB - 1) * NumberofReceipts) + 1,
                            LastReceiptNo = LB * NumberofReceipts,
                            Active = 1
                        };

                        Market.HandleBookReceipts.Add(HBookReceiptSave);
                        rowAffected = Market.SaveChanges();
                        LB++;
                    }
                }
                TempData["Msg"] = rowAffected > 0 ? "تم الحفظ بنجاح" : "لم يتم الحفظ";
                ModelState.Clear();
                return RedirectToAction("AddBook");
                }
            
            else
            {
                TempData["Msg"] = "لم يتم الحفظ";
            }
            return RedirectToAction("AddBook");
        }


        public ActionResult AllBooks()
        {
            List<BookModel> ListOfBooks = null;
            using (MarketEntities db = new MarketEntities())
            {
                ListOfBooks = (from b in db.BookTypes
                                               join h in db.HandleBookReceipts
                                               on b.BookTypeId equals h.BookTypeId
                                               join m in db.marketingrectypes
                                               on b.RecTypeId equals m.id
                                               where b.LicenseId == GLobalLicenseId && b.Active == 1 && m.Active == 1
                                               select new BookModel() { BookTypeId = b.BookTypeId, BookNo = b.BookNo, RecTypeName = m.name,
                                               FirstReceiptNo = h.FirstReceiptNo, LastReceiptNo = h.LastReceiptNo, Active = h.Active}).OrderBy(x=>x.RecTypeName).ToList<BookModel>();
            }
            return View(ListOfBooks);
        }

        public ActionResult EditBook(int /*this is BookTypeId*/id)
        {
            ViewBag.MyRecTypes = PopulateRecTypes();
            BookModel FilteredBooks;
            using (MarketEntities db = new MarketEntities())
            {
                FilteredBooks = (from b in db.BookTypes
                                 join h in db.HandleBookReceipts
                                 on b.BookTypeId equals h.BookTypeId
                                 join m in db.marketingrectypes
                                 on b.RecTypeId equals m.id
                                 where b.LicenseId == GLobalLicenseId && b.Active == 1 && m.Active == 1
                                 && b.BookTypeId == id
                                 select new BookModel()
                                 {
                                     BookTypeId = b.BookTypeId,
                                     RecTypeId = m.id,
                                     BookNo = b.BookNo,
                                     RecTypeName = m.name,
                                     FirstReceiptNo = h.FirstReceiptNo,
                                     LastReceiptNo = h.LastReceiptNo,
                                     Active = h.Active
                                 }).FirstOrDefault();
            }
            return PartialView(FilteredBooks);
        }
        [HttpPost]
        public ActionResult SaveEdit(BookModel bookModel)
        {
            if (bookModel.BookTypeId == 0)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            
            using (MarketEntities Market = new MarketEntities())
            {
                if (ModelState.IsValid)
                {
                    var BookToUpdate = Market.BookTypes.Where(x => x.BookTypeId == bookModel.BookTypeId).FirstOrDefault();
                    BookToUpdate.BookNo = bookModel.BookNo;
                    BookToUpdate.RecTypeId = bookModel.RecTypeId;
                    //------------------------------------//
                    var HBookToUpdate = Market.HandleBookReceipts.Where(x => x.BookTypeId == bookModel.BookTypeId).FirstOrDefault();
                    HBookToUpdate.FirstReceiptNo = bookModel.FirstReceiptNo;
                    HBookToUpdate.LastReceiptNo = bookModel.LastReceiptNo;
                    HBookToUpdate.Active = bookModel.IsActive ? 1 : 0;
                    TryUpdateModel(BookToUpdate);
                    int rowAffected = Market.SaveChanges();
                    TempData["Msg"] = rowAffected > 0 ? "تم الحفظ بنجاح" : "لم يتم الحفظ";
                }
                else
                {
                    TempData["Msg"] =  "لم يتم الحفظ";
                }
                
            }
            return RedirectToAction("AllBooks" , TempData["Msg"]);
        }

        public ActionResult AddReceiptType()
        {
            return View();
        }
        [HttpPost]
        public ActionResult AddReceiptType(ReceiptTypeModel RecModel)
        {
            if (ModelState.IsValid)
            {
                using (MarketEntities Market = new MarketEntities())
                {
                    marketingrectype recTypeSave = new marketingrectype();
                    recTypeSave.name = RecModel.ReceiptTypeName;
                    recTypeSave.Active = 1;
                    Market.marketingrectypes.Add(recTypeSave);
                    int rowAffected =Market.SaveChanges();
                    TempData["Msg"] = rowAffected > 0 ? "تم الحفظ بنجاح" : "لم يتم الحفظ";
                    ModelState.Clear();
                }
            }
            else
            {
                TempData["Msg"] = "لم يتم الحفظ";
            }
            return View();
        }
        //Remote Validation
        public JsonResult CheckReceiptTypeNameDuplication(string ReceiptTypeName)
        {
            bool IsExists = true;
            using (MarketEntities db = new MarketEntities())
            {
                IsExists = db.marketingrectypes.Where(x => x.name.Contains(ReceiptTypeName)).Any();
                if (IsExists)//If True this means Already Exists, then will switched to false because of the remote attribute requirments
                {
                    IsExists = false;
                }
                else
                {
                    IsExists = true;
                }
            }
            return Json(IsExists, JsonRequestBehavior.AllowGet);
        }

    }
}