using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ECBNewWeb.Models
{
    public class UserSiteModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "يجب اختيار الموظف")]
        [Remote("IsUserAvailble", "UserSite",AdditionalFields ="SiteId", ErrorMessage = "الموظف مسجل على موقع حالياً")]
        public Nullable<int> UserId { get; set; }
        [Required(ErrorMessage = "يجب اختيار الموقع")]
        [Remote("IsSiteAvailble", "UserSite",AdditionalFields ="UserId", ErrorMessage = "الموظف مسجل على هذا موقع")]
        public Nullable<int> SiteId { get; set; }
        public string Sitename { get; set; }

        public string Firstname { get; set; }
        public string Lastname { get; set; }

        public Nullable<System.DateTime> AssignDate { get; set; }
        public Nullable<int> Active { get; set; }
        public bool IsActive
        {
            get { return Active == 1; }
            set { Active = value ? 1 : 0; }
        }

        public IEnumerable<SelectListItem> myEmployee { get; set; }

        public IEnumerable<SelectListItem> mySites { get; set; }
    }
}