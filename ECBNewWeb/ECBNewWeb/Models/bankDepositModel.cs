using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ECBNewWeb.Models
{
    public class bankDepositModel
    {
        public int id { get; set; }
        public string BankName { get; set; }
        public Nullable<System.DateTime> date { get; set; }
        public Nullable<int> ApproveReceiptFK { get; set; }
    }
}