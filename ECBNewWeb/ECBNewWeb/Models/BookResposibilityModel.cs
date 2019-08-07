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
        public bool ParentAvailable { get; set; }
        [Required(ErrorMessage ="يجب اختيار اسم الموظف")]
        public Nullable<int> EmployeeId { get; set; }
        public int? BookTypeId { get; set; }
        public Nullable<int> HandleBookReceiptId { get; set; }
        public int? FirstReceiptNo { get; set; }
        public int? LastReceiptNo { get; set; }
        [Required(ErrorMessage ="يجب تحديد التاريخ")]
        [DataType(DataType.Date)]
        public Nullable<System.DateTime> ReceiveDate { get; set; }
        [Required(ErrorMessage = "يجب تحديد تاريخ تسليم العهدة")]
        [DataType(DataType.DateTime)]
        public Nullable<System.DateTime> DeliveryDate { get; set; }
        public Nullable<int> NextReceiptNo { get; set; }
        public Nullable<int> DoneFlag { get; set; }
        public List<SelectListItem> MyEmployee { get; set; }
        public IEnumerable<SelectListItem> MyRecTypes { get; set; }
        public int RecTypeId { get; set; }
        public int BookNo { get; set; }
        public string FullName { get; set; }
        public string ReceiptTypeName { get; set; }
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string lastName { get; set; }
        public int BookReceiptId { get; set; }
        public int DeliveryId { get; set; }
        public long DeliveryNo { get; set; }
        public List<SelectListItem> MyDeliveryNo { get; set; }
        public int? EmployeeNo { get; set; }
        public string FullEmployeeName { get; set; }
        public string SupervisorName { get; set; }
        public string SiteName { get; set; }
        public int? RequestId { get; set; }
        public long RequestNo { get; set; }
        public string BookState { get; set; }
    }
}