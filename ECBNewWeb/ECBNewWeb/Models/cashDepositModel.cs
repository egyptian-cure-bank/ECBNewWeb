using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ECBNewWeb.Models
{
    public class cashDepositModel
    {
        public int id { get; set; }
        public string receiptVoucher { get; set; }
        public Nullable<System.DateTime> Date { get; set; }
        public Nullable<int> ApproveReceiptFK { get; set; }
    }
}