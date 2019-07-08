using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.ComponentModel.DataAnnotations;
using ECBNewWeb.CustomValidation;

namespace ECBNewWeb.Models
{
    public class DonationData
    {

        public int id { get; set; }
        //Sites
        [Range(0,9999999999,ErrorMessage ="برجاء إختيار موقع")]
        public int ? SiteId { get; set; }
        public string SiteName { get; set; }
        public IEnumerable<SelectListItem> MySites { get; set; }
        //Receipts
        [Range(1,9999999999999, ErrorMessage = "برجاء إختار دفتر")]
        public int? RecId { get; set; }
        public string RecName { get; set; }
        public IEnumerable<SelectListItem> MyReceipts { get; set; }
        //[Range(1, 9999999999999999, ErrorMessage = "Error In Receipt Number")]
        public string RecNumber { get; set; }
        public int no { get; set; }
        //Currency
        public int? CurrencyId { get; set; }
        [Required(AllowEmptyStrings =false, ErrorMessage = "برجاء إختيار عملة")]
        public string CurrencyName { get; set; }
        public IEnumerable<SelectListItem> MyCurrency { get; set; }
        //Donation Purposes
        [Range(1,999999999999999, ErrorMessage = "برجاء إختيار غرض")]
        public int? PurpId { get; set; }
        public string PurpName { get; set; }
        public IEnumerable<SelectListItem> MyPurposes { get; set; }
        //Payment Methods
        [Range(1,99999999999999, ErrorMessage = "برجاء إختيار طريقة الدفع")]
        public int? PaymentId { get; set; }
        public string PaymentName { get; set; }
        public IEnumerable<SelectListItem> MyPayments { get; set; }
        //Knowing Methods
        public int? KnowingId { get; set; }
        public string KnowingMethodName { get; set; }
        public IEnumerable<SelectListItem> MyKnowingMethods { get; set; }
        //////////////////
        public int DonorId { get; set; }
        [Required(AllowEmptyStrings =false,ErrorMessage ="برجاء إختيار إسم متبرع")]
        public string DonorName { get; set; }
        [Required(AllowEmptyStrings = false, ErrorMessage = "برجاء إختيار تاريخ")]
        public DateTime RecDate { get; set; }
        [Required(AllowEmptyStrings =false,ErrorMessage ="برجاء كتابة مبلغ")]
        [Range(1,9999999999999,ErrorMessage ="القيمة يجب ان تكون اكبر من الصفر")]
        public decimal ? Amount { get; set; }
        //Cheque Bank Information
        public string BankInfoChecked { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "برجاء إختيار بنك")]
        public int ChequeBankId { get; set; }
        public string ChequeBankName { get; set; }
        public IEnumerable<SelectListItem> MyChequeBanks { get; set; }

        //[Remote("CheckChequeNumberValidation", "Donation", AdditionalFields = "BankInfoChecked", ErrorMessage = "برجاء كتابة رقم الشيك")]
        [Required(AllowEmptyStrings =false,ErrorMessage ="برجاء كتابة رقم الشيك")]
        public string ChequeNumber { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "برجاء كتابة تاريخ إستحقاق الشيك")]
        public DateTime ChequeDate { get; set; }
        public DateTime? MaxAssignDate { get; set; }

        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string LastName { get; set; }


    }
}