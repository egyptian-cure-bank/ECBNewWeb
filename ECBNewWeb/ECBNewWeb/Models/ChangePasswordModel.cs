using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace ECBNewWeb.Models
{
    public class ChangePasswordModel
    {
        public int UserId { get; set; }
        public string UserName { get; set; }
        [DataType(DataType.Password)]
        [Remote("ValidateOldPassword","Account", ErrorMessage = "خطأ بكلمة المرور")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "برجاء كتابة كلمة المرور")]
        public string OldPassword { get; set; }
        [DataType(DataType.Password)]
        [Required(AllowEmptyStrings = false, ErrorMessage = "برجاء كتابة كلمة المرور الجديدة")]
        public string NewPassword { get; set; }
        [Required(AllowEmptyStrings = false, ErrorMessage = "برجاء كتابة كلمة المرور الجديدة")]
        [DataType(DataType.Password)]
        [System.ComponentModel.DataAnnotations.Compare("NewPassword", ErrorMessage = "كلمة المرور غير متطابقة")]
        public string ConfirmPassword { get; set; }
    }
}