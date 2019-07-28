using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ECBNewWeb.Models
{
    public class BookRequestsModel
    {
        public int RequestId { get; set; }
        public long RequestNo { get; set; }
        public int EmployeeId { get; set; }
        public Nullable<System.DateTime> RequestDate { get; set; }
        public Nullable<System.DateTime> TimeStamp { get; set; }
        public Nullable<int> Active { get; set; }
    }
}