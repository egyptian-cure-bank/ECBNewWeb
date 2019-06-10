using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ECBNewWeb.Controllers
{
    public class CurrencyController : Controller
    {
        // GET: Currency
        public ActionResult AddCurrency()
        {
            return View();
        }

        public ActionResult CurrenyConversion()
        {
            return View();
        }
    }
}