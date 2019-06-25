using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ECBNewWeb.Models
{
    public class MarketGroupBriefModel
    {
        public bool UserNameCheck { get; set; }
        public bool DateCheck { get; set; }
        public bool RecTypeCheck { get; set; }
        [Required(AllowEmptyStrings = false, ErrorMessage = "يجب إختيار عملة")]
        public int UserId { get; set; }
        public string UserName { get; set; }
        public IEnumerable<SelectListItem> MyUserList { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public int RecTypeId { get; set; }
        public string RecType { get; set; }
        public IEnumerable<SelectListItem> MyRecTypes { get; set; }
        public int BookNumber { get; set; }
        public bool SiteCheck { get; set; }
        public int SiteId { get; set; }
        public string SiteName { get; set; }
        public IEnumerable<SelectListItem> MySites { get; set; }
        public bool PurposeCheck { get; set; }
        public int DonationPurposeId { get; set; }
        public string DonationPurposeName { get; set; }
        public IEnumerable<SelectListItem> MyDonationPurpose { get; set; }
    }
}