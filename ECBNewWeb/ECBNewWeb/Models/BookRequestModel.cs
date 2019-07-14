using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ECBNewWeb.Models
{
    public class BookRequestModel
    {
        public int RequestId { get; set; }
        public long RequestNo { get; set; }
        public List<SelectListItem> MyRequests { get; set; }
        public int SupervisorId { get; set; }
        public string SupervisorName { get; set; }
        public int EmployeeId { get; set; }
        public string FullEmployeeName { get; set; }
        public int SiteId { get; set; }
        public string SiteName { get; set; }
        public int HandleBookReceiptId { get; set; }
        public DateTime ReceiveDate { get; set; }
        public int? RecTypeId { get; set; }
        public string BookTypeName { get; set; }
        public int? Amount { get; set; }
    }
}