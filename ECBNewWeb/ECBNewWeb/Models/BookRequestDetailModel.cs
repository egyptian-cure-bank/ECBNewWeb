using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ECBNewWeb.Models
{
    public class BookRequestDetailModel
    {
        public int Id { get; set; }
        public Nullable<long> RequestNo { get; set; }
        public Nullable<int> ReceiptTypeId { get; set; }
        public string bookTypeName { get; set; }
        public Nullable<int> Amount { get; set; }
        public Nullable<int> SupervisorApproval { get; set; }
        public Nullable<int> FinanceApproval { get; set; }
        public Nullable<int> EmployeeReceive { get; set; }
        public Nullable<System.DateTime> RequestDate { get; set; }
        public int ? EmployeeId { get; set; }
        public string FullEmployeeName { get; set; }
        public int[] arr_Amount { get; set; }
        public int[] receiptTypeId { get; set; }
    }
}