using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace ECBNewWeb.Models
{
    public class MarketingLicensesModel
    {
        public int Id { get; set; }
        [Required(AllowEmptyStrings =false,ErrorMessage ="برجاء كتابة إسم الترخيص")]
        public string LicenseName { get; set; }

        [Required(AllowEmptyStrings =false,ErrorMessage ="برجاء إختيار تاريخ بدء الترخيص")]
        public Nullable<System.DateTime> FromDate { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "برجاء إختيار تاريخ نهاية الترخيص")]
        public Nullable<System.DateTime> ToDate { get; set; }
        public Nullable<System.DateTime> CreatedDate { get; set; }
        public Nullable<int> Active { get; set; }
    }
}