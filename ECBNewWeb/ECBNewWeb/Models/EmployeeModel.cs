using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ECBNewWeb.Models
{
    public class EmployeeModel
    {
        public int EmployeeId { get; set; }
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string LastName { get; set; }
        public Nullable<int> DepartmentId { get; set; }
        public Nullable<int> ParentEmployeeId { get; set; }
        public string NationalId { get; set; }
        public string MobileNumber { get; set; }
        public string EmailAddress { get; set; }
        public string NickName { get; set; }
        public Nullable<System.DateTime> CreatedDate { get; set; }
        public Nullable<int> Active { get; set; }
        public Nullable<int> EmployeeNo { get; set; }

        public string FullName { get; set; }
    }
}