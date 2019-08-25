using ECBNewWeb.DataAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ECBNewWeb.Models
{
    public class RolesModel
    {
        public int RoleID { get; set; }
        public string RoleArabicName { get; set; }
        public string RoleDescription { get; set; }
        public string Comments { get; set; }
        public bool IsSystemGenerated { get; set; }
        //public virtual ICollection<ParentFacilityRole> ParentFacilityRoles { get; set; }
        //public virtual ICollection<SubFacilityRole> SubFacilityRoles { get; set; }
        public virtual ICollection<UserRole> UserRoles { get; set; }
    }
}