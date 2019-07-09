using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.ComponentModel.DataAnnotations;
using ECBNewWeb.CustomValidation;

namespace ECBNewWeb.Models
{
    public class EmployeeModel
    {
        public int EmployeeId { get; set; }

        [Required(AllowEmptyStrings =false,ErrorMessage ="برجاء كتابة الإسم الأول")]
        public string FirstName { get; set; }

        [Required(AllowEmptyStrings =false,ErrorMessage ="برجاء كتابة الإسم الثاني")]
        public string MiddleName { get; set; }

        [Required(AllowEmptyStrings =false,ErrorMessage ="برجاء كتابة لقب العائلة")]
        public string LastName { get; set; }

        [Required(AllowEmptyStrings =false,ErrorMessage ="برجاء إختيار إدارة")]
        [Range(1,9999999999999999, ErrorMessage = "برجاء إختيار إدارة")]
        public int DepartmentId { get; set; }
        public string DepartmentName { get; set; }
        public IEnumerable<SelectListItem> MyDepartments { get; set; }

        public Nullable<int> ParentEmployeeId { get; set; }
        public string ParentEmployeeName { get; set; }
        public IEnumerable<SelectListItem>MyParentEmployees { get; set; }
        //[MinLength(14,ErrorMessage = "الرقم القومي أربعة عشر رقماَ")]
        //[MaxLength(14,ErrorMessage = "الرقم القومي أربعة عشر رقماَ")]
        [Remote("NationalIdValidation", "Employees",ErrorMessage = "الرقم القومي أربعة عشر رقماَ")]
        [Required(AllowEmptyStrings=false,ErrorMessage = "الرقم القومي لايمكن أن يكون فارغ")]
        public double? NationalId { get; set; }

        [Required(AllowEmptyStrings =false,ErrorMessage ="برجاء كتابة رقم تليفون")]
        public string MobileNumber { get; set; }

        [EmailAddress(ErrorMessage = "ايميل غير صحيح")]
        public string EmailAddress { get; set; }
        public string NickName { get; set; }
        public Nullable<System.DateTime> CreatedDate { get; set; }
        public Nullable<int> Active { get; set; }
        public bool IsActive
        {
            get { return Active == 1; }
            set { Active = value ? 1 : 0; }
        }
        public Nullable<int> EmployeeNo { get; set; }

        public string FullName { get; set; }
        public string job { get; set; }
    }
}