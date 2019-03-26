using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;
using ECBNewWeb.DataAccess;

namespace ECBNewWeb.CustomAuthentication
{
    public class CustomRole : RoleProvider
    {
        /// <summary>  
        ///   
        /// </summary>  
        /// <param name="username"></param>  
        /// <param name="roleName"></param>  
        /// <returns></returns>  
        public override bool IsUserInRole(string username, string roleName)
        {
            var userRoles = GetRolesForUser(username);
            return userRoles.Contains(roleName);
        }

        /// <summary>  
        ///   
        /// </summary>  
        /// <param name="username"></param>  
        /// <returns></returns>  
        public override string[] GetRolesForUser(string username)
        {
            if (!HttpContext.Current.User.Identity.IsAuthenticated)
            {
                return null;
            }

            var userRoles = new List<string>();

            using (AuthenticationEntities dbContext = new AuthenticationEntities())
            {
                var selectedUser = (from us in dbContext.LogIns/*.Include("UserRoles")*/
                                    where string.Compare(us.username, username, StringComparison.OrdinalIgnoreCase) == 0
                                    select us).FirstOrDefault();


                if (selectedUser != null)
                {
                    var currentRoleId = (dbContext.UserRoles.Where(x => x.LogIn.id == selectedUser.id).Select(s => s.RoleID).ToList());
                    userRoles = dbContext.Roles.Where(x => currentRoleId.Contains(x.RoleID)).Select(x => x.RoleName).ToList();
                }

                return userRoles.ToArray();
            }


        }



        #region Overrides of Role Provider  

        public override string ApplicationName
        {
            get
            {
                throw new NotImplementedException();
            }

            set
            {
                throw new NotImplementedException();
            }
        }

        public override void AddUsersToRoles(string[] usernames, string[] roleNames)
        {
            throw new NotImplementedException();
        }

        public override void CreateRole(string roleName)
        {
            throw new NotImplementedException();
        }

        public override bool DeleteRole(string roleName, bool throwOnPopulatedRole)
        {
            throw new NotImplementedException();
        }

        public override string[] FindUsersInRole(string roleName, string usernameToMatch)
        {
            throw new NotImplementedException();
        }

        public override string[] GetAllRoles()
        {
            using (AuthenticationEntities db = new AuthenticationEntities())
            {
                return db.Roles.Select(r => r.RoleName).ToArray<string>();
            }
        }

        public override string[] GetUsersInRole(string roleName)
        {
            throw new NotImplementedException();
        }


        public override void RemoveUsersFromRoles(string[] usernames, string[] roleNames)
        {
            throw new NotImplementedException();
        }

        public override bool RoleExists(string roleName)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}