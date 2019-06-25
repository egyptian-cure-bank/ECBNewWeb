using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace ECBNewWeb.Models
{
    public class LoginViewModel
    {
        public int UserId { get; set; }
        public string UserName { get; set; }
        [DataType(DataType.Password)]
        public string Password { get; set; }
        public bool RememberMe { get; set; }
        public string FullName { get; set; }
    }
    public class CustomSerializeModel
    {
        public int UserId { get; set; }
        public string UserName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int DeptId { get; set; }
        public List<string> RoleName { get; set; }

    }
}