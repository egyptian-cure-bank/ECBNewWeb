using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace ECBNewWeb.Models
{
    public class ReceiptTypeModel
    {
        [Required(AllowEmptyStrings =false,ErrorMessage ="برجاء كتابة إسم الدفتر")]
        [Remote("CheckReceiptTypeNameDuplication", "Book", ErrorMessage = "نوع الدفتر موجود بالفعل")]
        public string ReceiptTypeName { get; set; }
        public int Active { get; set; }
    }
}