using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ECBNewWeb.CustomAuthentication;
using System.Web.Security;
using ECBNewWeb.DataAccess;
using ECBNewWeb.Models;

namespace ECBNewWeb.Controllers
{
    public class UserRolesController : Controller
    {
        // GET: UserRoles
        public ActionResult AddUserRoles()
        {
            ViewBag.Rolelist = PopulateRoles();
            return View();
        }
        [HttpPost]
        public ActionResult AdduserRoles(UserRoleModel model)
        {
           
            if (ModelState.IsValid)
            {
                int rowAffected = 0;
                using (AuthenticationEntities db= new AuthenticationEntities())
                {

                    foreach(var item in model.roleArr)
                    {
                        
                        var role = new UserRole()
                        {
                            UserID = model.EmployeeID,
                            RoleID = item
                        };
                        db.UserRoles.Add(role);
                        rowAffected = db.SaveChanges();
                        TempData["Msg"] = rowAffected > 0 ? "تم الحفظ بنجاح" : "لم يتم الحفظ";
                    }

                }

            }
            else
            {
                TempData["Msg"] =  "لم يتم الحفظ";
            }
            return RedirectToAction("AddUserRoles");
        }

        [HttpGet]
        public ActionResult AllUserRoles()
        {
            MarketEntities db = new MarketEntities();
            var m = db.Database.SqlQuery<UserRoleModel>("select UserID,(e.FirstName +' ' +e.MiddleName + ' ' + e.LastName) as FullName, RoleName = STUFF((select ', ' + r.RoleName from UserRoles ur1 inner join Roles r on ur1.RoleID = r.RoleID where ur1.UserID = ur2.UserID FOR XML PATH('')), 1, 2, '') from UserRoles ur2 inner join login l on ur2.UserID = l.id inner join Employees e on l.employee_id = e.EmployeeId group by ur2.UserID , e.FirstName , e.LastName , e.MiddleName ");
            var userRoles = m.ToList();
            return View();
        }


        public JsonResult AutoCompleteEmployee(string prefix)
        {
            using (MarketEntities db = new MarketEntities())
            {
                List<EmployeeModel> Employees = (from d in db.Employees
                                                 join l in db.UserLogins on d.EmployeeId equals l.employee_id
                                                where d.Active == 1
                                                && (d.FirstName + d.MiddleName + d.LastName).Contains(prefix)
                                                select new EmployeeModel() { FullName = d.FirstName + " " + d.MiddleName + " " + d.LastName, EmployeeId = l.id }).ToList<EmployeeModel>();
                return Json(Employees, JsonRequestBehavior.AllowGet);
            }

        }

        public List<SelectListItem> PopulateRoles()
        {
            List<SelectListItem> Items = new List<SelectListItem>();
            using (AuthenticationEntities db = new AuthenticationEntities())
            {
                List<RolesModel> MyRoles = db.Database.SqlQuery<RolesModel>("SELECT RoleID,RoleName FROM Roles").ToList();
                SelectListItem DisabledList = new SelectListItem()
                {
                    Text = " ",
                    Value = " -1",
                    Selected = true
                };
                //Items.Add(DisabledList);
                foreach (RolesModel r in MyRoles)
                {
                    SelectListItem selectList = new SelectListItem()
                    {
                        Text = r.RoleName,
                        Value = r.RoleID.ToString(),
                    };

                    Items.Add(selectList);
                }
            }
            return Items;
        }
    }
}