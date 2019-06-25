using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ECBNewWeb.Models
{
    public class BookResposibilityModel
    {
        public int RespId { get; set; }
        [Required(ErrorMessage ="يجب اختيار اسم الموظف")]
        public Nullable<int> EmployeeId { get; set; }
        public Nullable<int> HandleBookReceiptId { get; set; }

        [Required(ErrorMessage ="يجب تحديد التاريخ")]
        [DataType(DataType.Date)]
        public Nullable<System.DateTime> ReceiveDate { get; set; }
        public Nullable<System.DateTime> DeliveryDate { get; set; }


        public Nullable<int> PartialBookIndicator { get; set; }
        public Nullable<int> ParentEmployeeId { get; set; }
        public Nullable<int> NextReceiptNo { get; set; }
        public Nullable<int> DoneFlag { get; set; }

        public List<SelectListItem> MyEmployee { get; set; }

        public IEnumerable<SelectListItem> MyRecTypes { get; set; }
        public int RecTypeId { get; set; }

        public int BookNo { get; set; }

        public string FullName { get; set; }
        public string ReceiptTypeName { get; set; }
        public string FirstName { get; set; }

        public string lastName { get; set; }

        public int BookReceiptId { get; set; }

        public int RespId2 { get; set; }
        public Nullable<System.DateTime> ReceiveDate2 { get; set; }
        public Nullable<System.DateTime> DeliveryDate2 { get; set; }
        public Nullable<int> EmployeeId2 { get; set; }






    }
}