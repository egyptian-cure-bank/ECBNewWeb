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
    
    public partial class SubFacilityRole
    {
        public int SubFacilityRoleId { get; set; }
        public Nullable<int> SubFacilityId { get; set; }
        public Nullable<int> RoleId { get; set; }
    
        public virtual SubFacility SubFacility { get; set; }
        public virtual Role Role { get; set; }
    }
}
