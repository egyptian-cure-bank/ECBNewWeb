using ECBNewWeb.DataAccess;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace ECBNewWeb.Models
{
    public class UserRoleModel
    {
        public int UserRoleID { get; set; }
        public int UserID { get; set; }
        public int ? RoleID { get; set; }

        public string RoleName { get; set; }
        [Required(AllowEmptyStrings = false, ErrorMessage ="يجب اختيار الموظف")]
        public int EmployeeID { get; set; }
        [Required(AllowEmptyStrings = false, ErrorMessage = "يجب اختيار اسم الموظف")]
        public string EmployeeName { get; set; }
        public virtual Role Role { get; set; }
        [Required(ErrorMessage = "يجب اختيار الصلاحيات")]
        public int [] roleArr { get; set; }
        public virtual login login { get; set; }
    }
}