using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ECBNewWeb.CustomAuthentication;
using System.Web.Security;
using ECBNewWeb.DataAccess;
using ECBNewWeb.Models;
using System.Data.SqlClient;

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
            AuthenticationEntities db = new AuthenticationEntities();
            MarketEntities mdb = new MarketEntities();
            var AllUserRoles = db.UserRoles
                .Join(db.Roles, ur => ur.RoleID, r => r.RoleID, (ur, r) => new { Roles = r, UserRole = ur })
                .GroupBy(x => x.UserRole.UserID)
                .Join(db.logins , x => x.Key  , l => l.id , (x , l) => new { login = l , UserRole = x })
                               .Select(c => new
                               {
                                   UserID = c.UserRole.Key,
                                   FirstName = c.login.FirstName , 
                                   LastName = c.login.LastName,
                                   RoleName = c.UserRole.Select(x => x.Roles.RoleName)
                               }).ToList()
                               .Select(x => new UserRoleModel()
                               {
                                   EmployeeName = x.FirstName + ' ' + x.LastName,
                                   UserID = x.UserID,
                                   RoleName = String.Join(", ", x.RoleName),
                               }
                              )
                              .ToList<UserRoleModel>();


            return View(AllUserRoles);
        }

        [HttpGet]
        public ActionResult EditUserRoles(int id)
        {
            using (AuthenticationEntities db = new AuthenticationEntities())
            {
                var userRole = (from d in db.UserRoles
                                join l in db.logins on d.UserID equals l.id
                                 where d.UserID == id
                                 select new 
                                 {
                                     fullName = (l.FirstName +" " +l.LastName),
                                     d.RoleID,
                                     d.Role.RoleName
                                 });
                ViewBag.Rolelist = PopulateRoles();
                UserRoleModel model = new UserRoleModel();
                model.UserID = id;
                model.EmployeeName = userRole.Select(x => x.fullName).FirstOrDefault();
                model.roleArr = userRole.Select(x => x.RoleID).ToArray();
                return PartialView(model);
            }
        }
        [HttpPost]
        public ActionResult EditUserRoles(UserRoleModel userrole)
        {
            int rowAffected = 0;
            if (ModelState.IsValid)
            {
                using (AuthenticationEntities db = new AuthenticationEntities())
                {
                    rowAffected =  db.Database.ExecuteSqlCommand(@"DELETE FROM UserRoles WHERE UserID = @id" , new SqlParameter("@id", userrole.UserID));

                    foreach (var item in userrole.roleArr)
                    {

                        var role = new UserRole()
                        {
                            UserID = userrole.UserID,
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
                TempData["Msg"] = "لم يتم الحفظ";
            }
            return RedirectToAction("AllUserRoles");
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
                List<RolesModel> MyRoles = db.Database.SqlQuery<RolesModel>("SELECT RoleID,RoleArabicName FROM Roles").ToList();
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
                        Text = r.RoleArabicName,
                        Value = r.RoleID.ToString(),
                    };

                    Items.Add(selectList);
                }
            }
            return Items;
        }
    }
}