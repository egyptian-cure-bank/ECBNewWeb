using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ECBNewWeb.Models
{
    public class DonationData
    {
        //Sites
        public int SiteId { get; set; }
        public string SiteName { get; set; }
        public IEnumerable<SelectListItem> MySites { get; set; }
        //Receipts
        public int RecId { get; set; }
        public string RecName { get; set; }
        public IEnumerable<SelectListItem> MyReceipts { get; set; }
        //Currency
        public int CurrencyId { get; set; }
        public string CurrencyName { get; set; }
        public IEnumerable<SelectListItem> MyCurrency { get; set; }
        //Donation Purposes
        public int PurpId { get; set; }
        public string PurpName { get; set; }
        public IEnumerable<SelectListItem> MyPurposes { get; set; }
        //Payment Methods
        public int PaymentId { get; set; }
        public string PaymentName { get; set; }
        public IEnumerable<SelectListItem> MyPayments { get; set; }
        //Knowing Methods
        public int KnowingId { get; set; }
        public string KnowingMethodName { get; set; }
        public IEnumerable<SelectListItem> MyKnowingMethods { get; set; }
        //////////////////
        public int DonorId { get; set; }
        public string DonorName { get; set; }
        public DateTime RecDate { get; set; }
        public decimal Amount { get; set; }
        public int RecNumber { get; set; }
        public int? NextRecNumber { get; set; }


    }
}