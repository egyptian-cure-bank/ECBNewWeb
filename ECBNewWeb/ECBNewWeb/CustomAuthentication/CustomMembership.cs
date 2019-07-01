using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Security.Cryptography;
using ECBNewWeb.DataAccess;
using System.Text;

namespace ECBNewWeb.CustomAuthentication
{
    public class CustomMembership : MembershipProvider
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        /// 
        public override bool ValidateUser(string username, string password)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                return false;
            }
            SHA1 s = new SHA1CryptoServiceProvider();
            var bytes = Encoding.UTF8.GetBytes(password);
            var bytess = s.ComputeHash(bytes);
            var sos = Convert.ToBase64String(bytess);

            using (AuthenticationEntities dbContext = new AuthenticationEntities())
            {
                var user = (from us in dbContext.LogIns
                            where string.Compare(username, us.username, StringComparison.OrdinalIgnoreCase) == 0
                            && string.Compare(sos, us.password, StringComparison.OrdinalIgnoreCase) == 0
                            && us.active == true
                            select new { us }).FirstOrDefault();
                using (MarketEntities dbMarket = new MarketEntities())
                {
                    var AllInfo = (from e in dbMarket.Employees
                                   where e.EmployeeId == user.us.employee_id
                                   select new { user.us.id,user.us.userRole,user.us.username,user.us.department,user.us.employee_id, e }).FirstOrDefault();
                    return (user != null) ? true : false;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <param name="email"></param>
        /// <param name="passwordQuestion"></param>
        /// <param name="passwordAnswer"></param>
        /// <param name="isApproved"></param>
        /// <param name="providerUserKey"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        public override MembershipUser CreateUser(string username, string password, string email, string passwordQuestion, string passwordAnswer, bool isApproved, object providerUserKey, out MembershipCreateStatus status)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="username"></param>
        /// <param name="userIsOnline"></param>
        /// <returns></returns>
        public override MembershipUser GetUser(string username, bool userIsOnline)
        {
            using (MarketEntities dbMarket = new MarketEntities())
            {
                Employee Emp ;
                using (AuthenticationEntities dbContext = new AuthenticationEntities())
                {
                    var user = (from us in dbContext.LogIns
                                where string.Compare(username, us.username, StringComparison.OrdinalIgnoreCase) == 0
                                select us).FirstOrDefault();

                    if (user == null)
                    {
                        return null;
                    }
                    else
                    {
                        Emp = dbMarket.Employees.Where(x => x.EmployeeId == user.employee_id).FirstOrDefault();
                    }
                    var selectedUser = new CustomMembershipUser(user,Emp);
                    return selectedUser;
                }
            }
        }

        public override string GetUserNameByEmail(string email)
        {
            throw new NotImplementedException();
            //using (AuthenticationDB dbContext = new DataAccess.AuthenticationDB())
            //{
            //    string username = (from u in dbContext.Users
            //                       where string.Compare(email, u.Email) == 0
            //                       select u.Username).FirstOrDefault();

            //    return !string.IsNullOrEmpty(username) ? username : string.Empty;
            //}
        }

        #region Overrides of Membership Provider

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

        public override bool EnablePasswordReset
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public override bool EnablePasswordRetrieval
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public override int MaxInvalidPasswordAttempts
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public override int MinRequiredNonAlphanumericCharacters
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public override int MinRequiredPasswordLength
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public override int PasswordAttemptWindow
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public override MembershipPasswordFormat PasswordFormat
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public override string PasswordStrengthRegularExpression
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public override bool RequiresQuestionAndAnswer
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public override bool RequiresUniqueEmail
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public override bool ChangePassword(string username, string oldPassword, string newPassword)
        {
            throw new NotImplementedException();
        }

        public override bool ChangePasswordQuestionAndAnswer(string username, string password, string newPasswordQuestion, string newPasswordAnswer)
        {
            throw new NotImplementedException();
        }

        public override bool DeleteUser(string username, bool deleteAllRelatedData)
        {
            throw new NotImplementedException();
        }

        public override MembershipUserCollection FindUsersByEmail(string emailToMatch, int pageIndex, int pageSize, out int totalRecords)
        {
            throw new NotImplementedException();
        }

        public override MembershipUserCollection FindUsersByName(string usernameToMatch, int pageIndex, int pageSize, out int totalRecords)
        {
            throw new NotImplementedException();
        }

        public override MembershipUserCollection GetAllUsers(int pageIndex, int pageSize, out int totalRecords)
        {
            throw new NotImplementedException();
        }

        public override int GetNumberOfUsersOnline()
        {
            throw new NotImplementedException();
        }

        public override string GetPassword(string username, string answer)
        {
            throw new NotImplementedException();
        }

        public override MembershipUser GetUser(object providerUserKey, bool userIsOnline)
        {
            throw new NotImplementedException();
        }

        public override string ResetPassword(string username, string answer)
        {
            throw new NotImplementedException();
        }

        public override bool UnlockUser(string userName)
        {
            throw new NotImplementedException();
        }

        public override void UpdateUser(MembershipUser user)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}