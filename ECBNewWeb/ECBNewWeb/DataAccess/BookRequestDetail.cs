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
    
    public partial class BookRequestDetail
    {
        public int Id { get; set; }
        public Nullable<long> RequestNo { get; set; }
        public Nullable<int> ReceiptTypeId { get; set; }
        public Nullable<int> Amount { get; set; }
        public Nullable<int> SupervisorApproval { get; set; }
        public Nullable<int> FinanceApproval { get; set; }
        public Nullable<int> EmployeeReceive { get; set; }
    }
}