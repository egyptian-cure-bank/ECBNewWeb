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
    
    public partial class Employee
    {
        public int EmployeeId { get; set; }
        public Nullable<int> EmployeeNo { get; set; }
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string LastName { get; set; }
        public Nullable<int> DepartmentId { get; set; }
        public Nullable<int> ParentEmployeeId { get; set; }
        public Nullable<double> NationalId { get; set; }
        public string MobileNumber { get; set; }
        public string EmailAddress { get; set; }
        public string NickName { get; set; }
        public Nullable<System.DateTime> CreatedDate { get; set; }
        public Nullable<int> Active { get; set; }
    }
}
