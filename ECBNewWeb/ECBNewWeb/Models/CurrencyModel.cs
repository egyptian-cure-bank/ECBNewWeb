using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace ECBNewWeb.Models
{
    public class CurrencyModel
    {
        //Currency Definition
        public int CurrencyId { get; set; }
        [Remote("CheckCurrencyNameDuplication", "Currency",ErrorMessage ="هذه العملة موجودة بالفعل")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "برجاء كتابة إسم عملة")]
        public string CurrName { get; set; }
    }
}