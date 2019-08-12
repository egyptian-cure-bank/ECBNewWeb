using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ECBNewWeb.Models
{
    public class MenuModel
    {
        public int MenuId { get; set; }
        public string EnglishName { get; set;}
        public string ArabicName { get; set; }
        public string Description { get; set; }
        public string Action { get; set; }
        public string Controller { get; set; }
        public string Url { get; set; }
        public string CssClass { get; set; }
        public int ParentMenuId { get; set; }
        public int? Sorting { get; set; }
        public virtual MenuModel ParentMenuItem { get; set; }
        public List<MenuModel> Children { get; set; }
    }
}