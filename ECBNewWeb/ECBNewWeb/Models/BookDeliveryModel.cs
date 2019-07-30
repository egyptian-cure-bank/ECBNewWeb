using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ECBNewWeb.Models
{
    public class BookDeliveryModel
    {
        public int? RequestId { get; set; }
        public long RequestNo { get; set; }
        public List<SelectListItem> MyRequests { get; set; }
        public int DeliveryId { get; set; }
        public long DeliveryNo { get; set; }
        public List<SelectListItem> MyDeliveryNo { get; set; }
        public int? EmployeeNo { get; set; }
        public string FullEmployeeName { get; set; }
        public string SupervisorName { get; set; }
        public string SiteName { get; set; }
        public int ResponsibilityId { get; set; }
        public int[] TotalRespIds { get; set; }
        public int RecTypeId { get; set; }
        public string RecTypeName { get; set; }
        public List<SelectListItem> MyRecTypes { get; set; }
        public int BookTypeId { get; set; }
        public int BookNumber { get; set; }
        public List<SelectListItem> MyBookNumbers { get; set; }
        public int FinanceApproval { get; set; }
        public int SupervisorApproval { get; set; }
    }
}