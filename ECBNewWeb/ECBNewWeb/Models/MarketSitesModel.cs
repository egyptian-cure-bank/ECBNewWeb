using ECBNewWeb.DataAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ECBNewWeb.Models
{
    public class MarketSitesModel
    {
        public int id { get; set; }
        public string sitename { get; set; }
        public Nullable<int> Active { get; set; }
        public virtual ICollection<market> markets { get; set; }

    }
}