using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace ECBNewWeb.Models
{
    public class Governtment
    {
        public int GoverntmentId { get; set; }
        public string GovernmentName { get; set; }
        public IEnumerable<SelectListItem> MyGovernments { get; set; }
    }
}