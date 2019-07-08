using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ECBNewWeb.Models
{
    public class ApproveReceiptModel
    {
        public int approveReceiptId { get; set; }
        public Nullable<int> marketId { get; set; }
        public Nullable<System.DateTime> approveDate { get; set; }
        public Nullable<int> depositType { get; set; }
    }
}