using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Security.Principal;

namespace ECBNewWeb.CustomAuthentication
{
    public class CustomPrincipal : IPrincipal
    {
        public int UserId { get; set; }
        public string UsrName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public List<string> Roles { get; set; }

        public IIdentity Identity
        {
            get; private set;
        }
        public bool IsInRole(string role)
        {
            if (Roles.Any(r => role.Contains(r)))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public CustomPrincipal(string username)
        {
            Identity = new GenericIdentity(username);
        }
    }
}