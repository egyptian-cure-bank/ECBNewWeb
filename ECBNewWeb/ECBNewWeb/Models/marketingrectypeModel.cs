using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ECBNewWeb.Models
{
    public class marketingrectypeModel
    {
        public int id { get; set; }
        public string name { get; set; }
        public Nullable<int> Active { get; set; }

        public int recTypeCount { get; set; }
    }
}