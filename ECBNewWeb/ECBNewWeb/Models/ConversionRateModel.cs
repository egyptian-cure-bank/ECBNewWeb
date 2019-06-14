using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ECBNewWeb.Models
{
    public class ConversionRateModel
    {
        //Source Currency Model
        public int SourceCurrencyId { get; set; }
        public string SourceCurrencyName { get; set; }
        public IEnumerable<SelectListItem> SourceMyCurrencies { get; set; }
        //Target Currency Model
        public int TargetCurrencyId { get; set; }
        public string TargetCurrencyName { get; set; }
        public IEnumerable<SelectListItem> TargetMyCurrencies { get; set; }
        //Currency Conversions
        //[RegularExpression(@"^[0-9]+(\.[0-9]{1,2})$", ErrorMessage ="معامل التحويل لا يقبل أكثر من علمتين عشريتين")]
        [RegularExpression(@"^\d+\.\d{1,2}$", ErrorMessage = "معامل التحويل لا يقبل أكثر من علمتين عشريتين")]
        [Range(1, 9999999999999999.99, ErrorMessage = "القيمة تبدأ من واحد")]
        public decimal ConversionRate { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public int Active { get; set; }
    }
}