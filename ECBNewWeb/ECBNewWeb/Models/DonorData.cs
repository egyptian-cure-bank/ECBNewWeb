using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.ComponentModel.DataAnnotations;

namespace ECBNewWeb.Models
{
    public class DonorData
    {
        //Governments

        [Required(ErrorMessage = "يجب اختيار المحافظة")]
        public int? GovernmentId { get; set; }
        public string GovernmentName { get; set; }
        public IEnumerable<SelectListItem> MyGovernments { get; set; }
        //Centers
        [Required(ErrorMessage = "يجب اختيار المركز")]
        public Nullable<int> CenterId { get; set; }
        public int GovernId { get; set; }
        public string CenterName { get; set; }
        public IEnumerable<SelectListItem> MyCenters { get; set; }
        //Genders
        [Required(ErrorMessage = "يجب اختيار النوع")]
        public string GenderValue { get; set; }
        public IEnumerable<SelectListItem> Gender { get; set; }
        //TypeContact
        public int? ContactId { get; set; }
        public string TypeContactName { get; set; }
        public string ContactValue { get; set; }
        public IEnumerable<SelectListItem> TypeContacts { get; set; }
        //Motabare3
        public int? DonorOfId { get; set; }
        public string DonorOfName { get; set; }
        public string Motabre3Value { get; set; }
        public IEnumerable<SelectListItem> DonorOFs { get; set; }
        //Freq
        public int? FreqId { get; set; }
        public string FreqName { get; set; }
        public string FreqValue { get; set; }
        public IEnumerable<SelectListItem> Freqs { get; set; }
        //Donors
        public int Id { get; set; }

        [Required(ErrorMessage = "يجب ادخال الاسم")]
        public string DonorName { get; set; }
        public string Title { get; set; }

        [Required(ErrorMessage = "يجب ادخال رقم التليفون")]
        [MaxLength(11, ErrorMessage = "رقم التليفون غير صحيح")]
        [MinLength(11, ErrorMessage = "رقم التليفون غير صحيح")]
        [RegularExpression("^[0-9]*$", ErrorMessage = "رقم تليفون غير صحيح")]
        public string Tele { get; set; }
        public string Job { get; set; }
        public string WorkPlace { get; set; }
        public string Address { get; set; }
        [Required(ErrorMessage = "برجاء ادخال الايميل")]
        [EmailAddress(ErrorMessage = "ايميل غير صحيح")]
        public string Email { get; set; }
        public string Notes { get; set; }
    }
}