//------------------------------------------------------------------------------
// <auto-generated>
//    This code was generated from a template.
//
//    Manual changes to this file may cause unexpected behavior in your application.
//    Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace ECBNewWeb.DataAccess
{
    using System;
    using System.Collections.Generic;
    
    public partial class cashDeposit
    {
        public int id { get; set; }
        public string receiptVoucher { get; set; }
        public Nullable<System.DateTime> Date { get; set; }
        public Nullable<int> ApproveReceiptFK { get; set; }
    }
}
