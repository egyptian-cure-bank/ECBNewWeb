using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.ComponentModel.DataAnnotations;

namespace ECBNewWeb.Models
{
    public class BookModel
    {
        public int BookReceiptId { get; set; }
        public int BookTypeId { get; set; }
        [Range(1,99999999999999,ErrorMessage ="الرقم لايمكن ان يكون صفر")]
        public int BookNo { get; set; }
        [Required(AllowEmptyStrings = false,ErrorMessage ="يجب إختيار إسم دفتر")]
        public int RecTypeId { get; set; }
        public string RecTypeName { get; set; }
        public IEnumerable<SelectListItem> MyRecTypes { get; set; }
        [Required]
        public int LicenseId { get; set; }
        public int? Active { get; set; }
        public bool IsActive {
            get { return Active == 1; }
            set { Active = value ? 1 : 0; }
        }
        [Range(1, 99999999999999, ErrorMessage = "الرقم لايمكن ان يكون صفر")]
        [Required]
        public int? FirstReceiptNo { get; set; }
        [Range(1, 99999999999999, ErrorMessage = "الرقم لايمكن ان يكون صفر")]
        [Required]
        public int?LastReceiptNo { get; set; }

        public IEnumerable<SelectListItem> MyHandleBookReceipts { get; set; }
    }

}