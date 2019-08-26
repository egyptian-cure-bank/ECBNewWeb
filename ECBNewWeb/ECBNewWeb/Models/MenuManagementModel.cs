using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ECBNewWeb.Models
{
    public class MenuManagementModel
    {
        [Remote("IsRoleHasMenus", "Menus",ErrorMessage = "يوجد قوائم على هذا الدور الوظيفي")]
        public int RoleId { get; set; }
        [Required(AllowEmptyStrings =false,ErrorMessage ="برجاء كتابة إسم الدور الوظيفي")]
        public string RoleArabicName { get; set; }
        public string RoleEnglishName { get; set; }
        public string RoleDescription { get; set; }
        public List<SelectListItem> MyRoles { get; set; }
        public int id { get; set; }
        public string text { get; set; }
        public int ParentMenuId { get; set; }
        public List<MenuManagementModel> children { get; set; }
        public List<SelectListItem> MyParentMenus { get; set; }
    }
    public class MenuManagementDeleteModel
    {
        public int RoleId { get; set; }
        public string RoleArabicName { get; set; }
        public string RoleEnglishName { get; set; }
        public string RoleDescription { get; set; }
        public List<SelectListItem> MyRoles { get; set; }
        public int id { get; set; }
        public string text { get; set; }
        public int ParentMenuId { get; set; }
        public List<MenuManagementDeleteModel> children { get; set; }
        public List<SelectListItem> MyParentMenus { get; set; }
    }
}