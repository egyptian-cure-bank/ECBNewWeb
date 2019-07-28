using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;
using ECBNewWeb.DataAccess;

namespace ECBNewWeb.CustomAuthentication
{
    public class CustomMembershipUser : MembershipUser
    {
        #region User Properties  

        public int UserId { get; set; }
        public string UsrName { get; set; }
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string LastName { get; set; }
        public int? Department { get; set; }
        public int? EmployeeId { get; set; }
        public ICollection<UserRole> Roles { get; set; }
        public ICollection<Role> RoleName { get; set; }
        public List<string> AccessLevels { get; set; }

        #endregion

        public CustomMembershipUser(login user,Employee employee) : base("CustomMembership", user.username, user.id, string.Empty, string.Empty, string.Empty, true, false, DateTime.Now, DateTime.Now, DateTime.Now, DateTime.Now, DateTime.Now)
        {
            UserId = user.id;
            UsrName = user.username;
            FirstName = employee.FirstName;
            MiddleName = employee.MiddleName;
            LastName = employee.LastName;
            Department = user.department;
            EmployeeId = user.employee_id;
            Roles = user.UserRoles;
            //LoginEntities db = new LoginEntities();
            //var _accessLevels = db.Grants.Where(u => u.UserId == UserId).Select(a => a.AccessLevel).ToList();
            //AccessLevels = db.AccessLevels.Where(a =>_accessLevels.Contains(a.Id)).Select(a=>a.AccessLevel1).ToList();
        }
    }
}