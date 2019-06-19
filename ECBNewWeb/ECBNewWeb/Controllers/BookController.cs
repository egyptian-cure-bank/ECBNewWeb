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
        public ActionResult SaveBook(BookModel bookModel)
        {
            if (ModelState.IsValid)
            {
                using (MarketEntities Market = new MarketEntities())
                {
                    BookType BookTypeSave = new BookType();
                    BookTypeSave.BookNo = bookModel.BookNo;
                    BookTypeSave.RecTypeId = bookModel.RecTypeId;
                    BookTypeSave.LicenseId = GLobalLicenseId;
                    BookTypeSave.Active = 1;
                    Market.BookTypes.Add(BookTypeSave);
                    Market.SaveChanges();
                    //save values to handlebookreceipts table
                    HandleBookReceipt HBookReceiptSave = new HandleBookReceipt();
                    HBookReceiptSave.BookTypeId = BookTypeSave.BookTypeId;
                    HBookReceiptSave.FirstReceiptNo = bookModel.FirstReceiptNo;
                    HBookReceiptSave.LastReceiptNo = bookModel.LastReceiptNo;
                    HBookReceiptSave.Active = 1;
                    Market.HandleBookReceipts.Add(HBookReceiptSave);
                    Market.SaveChanges();
                    ModelState.Clear();
                    return RedirectToAction("AddBook", bookModel);
                }
            }
            return RedirectToAction("AddBook",bookModel);
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
                var BookToUpdate = Market.BookTypes.Where(x=>x.BookTypeId==bookModel.BookTypeId).FirstOrDefault();
                BookToUpdate.BookNo = bookModel.BookNo;
                BookToUpdate.RecTypeId = bookModel.RecTypeId;
                //------------------------------------//
                var HBookToUpdate = Market.HandleBookReceipts.Where(x => x.BookTypeId == bookModel.BookTypeId).FirstOrDefault();
                HBookToUpdate.FirstReceiptNo = bookModel.FirstReceiptNo;
                HBookToUpdate.LastReceiptNo = bookModel.LastReceiptNo;
                HBookToUpdate.Active = bookModel.IsActive ? 1 : 0;
                TryUpdateModel(BookToUpdate);
                Market.SaveChanges();
            }
            return RedirectToAction("AllBooks");
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
                    Market.SaveChanges();
                    ModelState.Clear();
                }
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