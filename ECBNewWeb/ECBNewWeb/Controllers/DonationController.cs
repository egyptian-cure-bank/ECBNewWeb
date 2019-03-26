using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ECBNewWeb.CustomAuthentication;
using ECBNewWeb.DataAccess;

namespace ECBNewWeb.Controllers
{
    public class DonationController : Controller
    {
        [CustomAuthorize(AccessLevel = "CreateAddDonationsDonation,FullControlAddDonationDonation")]
        public ActionResult AddDonations()
        {
            return View("~/Views/Market/AddDonations.cshtml");
        }
        public JsonResult AutoCompleteDonor(string prefix)
        {
            using (MarketEntities db = new MarketEntities())
            {
                var DonorName = db.doners.Where(x => x.name.Contains(prefix)).Select(x => x.name).ToList();
                return Json(DonorName, JsonRequestBehavior.AllowGet);
            }
        }
    }
}