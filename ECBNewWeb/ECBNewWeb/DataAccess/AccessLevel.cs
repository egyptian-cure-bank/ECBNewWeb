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
    
    public partial class AccessLevel
    {
        public AccessLevel()
        {
            this.Grants = new HashSet<Grant>();
        }
    
        public int Id { get; set; }
        public string AccessLevel1 { get; set; }
    
        public virtual ICollection<Grant> Grants { get; set; }
    }
}
