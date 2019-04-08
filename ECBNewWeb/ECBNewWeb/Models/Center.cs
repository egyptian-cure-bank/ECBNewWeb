using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ECBNewWeb.Models
{
    public class Center
    {
        public int? CenterId { get; set; }
        public int? GovernId { get; set; }
        public string CenterName { get; set; }
        public IEnumerable<SelectListItem> MyCenters { get; set; }
    }
}