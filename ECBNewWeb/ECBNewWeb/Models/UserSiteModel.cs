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
        [Remote("IsUserAvailble", "UserSite", ErrorMessage = "الموظف موجود مسبقاً")]
        public Nullable<int> UserId { get; set; }
        [Required(ErrorMessage = "يجب اختيار الموقع")]
        public Nullable<int> SiteId { get; set; }
        public string Sitename { get; set; }

        public string Firstname { get; set; }
        public string Lastname { get; set; }

        public Nullable<System.DateTime> AssignDate { get; set; }
        public Nullable<int> Active { get; set; }

        public IEnumerable<SelectListItem> myEmployee { get; set; }

        public IEnumerable<SelectListItem> mySites { get; set; }
    }
}