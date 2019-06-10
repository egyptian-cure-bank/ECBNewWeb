using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ECBNewWeb.Controllers
{
    public class BookController : Controller
    {
        // GET: Book
        public ActionResult AddBook()
        {
            return View();
        }


        public ActionResult AddReceiptType()
        {
            return View();
        }

        public ActionResult HandleBookReceipts()
        {
            return View();
        }
    }
}