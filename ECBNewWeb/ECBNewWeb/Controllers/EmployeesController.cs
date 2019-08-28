using ECBNewWeb.CustomAuthentication;
using ECBNewWeb.DataAccess;
using ECBNewWeb.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using System.Data.SqlClient;
using System.Data.Objects.SqlClient;
using System.Net;
using System.Security.Cryptography;
using System.Text;

namespace ECBNewWeb.Controllers
{
    public class EmployeesController : Controller
    {
       
        private CustomMembershipUser UserInfo;
        
        // GET: Employees
        public ActionResult AddEmployee()
        {
            TempData["Controller"] = this.ControllerContext.RouteData.Values["controller"].ToString();
            if (User.Identity.IsAuthenticated)
            {
                UserInfo = (CustomMembershipUser)Membership.GetUser(HttpContext.User.Identity.Name, false);
                Session["CurrentUser"] = Membership.GetUser(HttpContext.User.Identity.Name, false);
            }
            EmployeeModel EmpModel = new EmployeeModel();
            EmpModel.MyDepartments = PopulateDepartments();
            
            return View(EmpModel);
        }
        public ActionResult AllEmployees()
        {
            TempData["Controller"] = this.ControllerContext.RouteData.Values["controller"].ToString();
            MarketEntities db = new MarketEntities();
            List<EmployeeModel> list = (from e in db.Employees
                                        join d in db.Departments on e.DepartmentId equals d.DepartmentId
                                        join p in db.Employees on e.ParentEmployeeId equals p.EmployeeId
                                        into u 
                                        from parentemp in u.DefaultIfEmpty()

                                        select new EmployeeModel()
                                        {
                                            EmployeeId = e.EmployeeId,
                                            EmployeeNo = e.EmployeeNo,
                                            FullName = e.FirstName + " " + e.MiddleName + " " + e.LastName,
                                            DepartmentName = d.DepartmentName,
                                            ParentEmployeeName = (parentemp.FirstName + " " + parentemp.MiddleName + " " + parentemp.LastName)?? String.Empty,
                                            Active = e.Active
                                        }).ToList<EmployeeModel>();
            return View(list);
        }
        private List<SelectListItem> PopulateDepartments()
        {
            List<SelectListItem> Items = new List<SelectListItem>();
            using (MarketEntities db = new MarketEntities())
            {
                List<EmployeeModel> MyDepartment = (from d in db.Departments
                                              where d.Active == 1
                                              select new EmployeeModel() { DepartmentId = d.DepartmentId, DepartmentName = d.DepartmentName }).ToList<EmployeeModel>();
                SelectListItem DisabledItem = new SelectListItem()
                {
                    Text = "",
                    Value = "-1"
                };
                Items.Add(DisabledItem);
                foreach (EmployeeModel item in MyDepartment)
                {
                    SelectListItem selectList = new SelectListItem()
                    {
                        Text = item.DepartmentName,
                        Value = item.DepartmentId.ToString()
                    };
                    Items.Add(selectList);
                }

            }
            return Items;
        }
        private List<SelectListItem> PopulateParentEmp()
        {
            List<SelectListItem> Items = new List<SelectListItem>();
            using (MarketEntities db = new MarketEntities())
            {
                List<EmployeeModel> MyParentEmp = (from d in db.Employees
                                                    where d.Active == 1
                                                    select new EmployeeModel() { ParentEmployeeId = d.EmployeeId, ParentEmployeeName = d.FirstName+" "+d.MiddleName+" "+d.LastName
                                                    }).ToList<EmployeeModel>();
                SelectListItem DisabledItem = new SelectListItem()
                {
                    Text = "",
                    Value = "-1"
                };
                Items.Add(DisabledItem);

                foreach (EmployeeModel item in MyParentEmp)
                {
                    SelectListItem selectList = new SelectListItem()
                    {
                        Text = item.ParentEmployeeName,
                        Value = item.ParentEmployeeId.ToString()
                    };
                    Items.Add(selectList);
                }

            }
            return Items;
        }
        public JsonResult AutoCompleteParentEmployee(string prefix)
        {
            using (MarketEntities db = new MarketEntities())
            {
               List<EmployeeModel> MyParent = (from d in db.Employees
                            where d.Active == 1
                            && (d.FirstName + d.MiddleName  + d.LastName).Contains(prefix)
                            select new EmployeeModel() {FullName = d.FirstName + " " + d.MiddleName + " " + d.LastName, EmployeeId = d.EmployeeId }).ToList<EmployeeModel>();
                return Json(MyParent, JsonRequestBehavior.AllowGet);
            }
            
        }

        public List<SelectListItem> GetJob(int deptid)
        {
            List<SelectListItem> Jobs = new List<SelectListItem>();
            using (MarketEntities db = new MarketEntities())
            {
                List<EmployeeModel> MyJobList = (from j in db.Jobs
                                                 where j.DepartmentId == deptid
                                                 select new EmployeeModel() { JobId = j.JobId, JobArabicName = j.JobArabicName }
                                                 ).ToList<EmployeeModel>();
                foreach (EmployeeModel item in MyJobList)
                {
                    SelectListItem selectList = new SelectListItem()
                    {
                        Text = item.JobArabicName,
                        Value = item.JobId.ToString()
                    };
                    Jobs.Add(selectList);
                }
            }
            return Jobs;
        }
        // get job by department Json
        public JsonResult GetJobList(int deptid)
        {
            List<SelectListItem> Jobs = new List<SelectListItem>();
            using (MarketEntities db = new MarketEntities())
            {
                List<EmployeeModel> MyJobList = (from j in db.Jobs
                                                 where j.DepartmentId == deptid
                                                 select new EmployeeModel() {JobId = j.JobId,JobArabicName = j.JobArabicName }
                                                 ).ToList<EmployeeModel>();
                SelectListItem DisabledItem = new SelectListItem()
                {
                    Text = "",
                    Value = "-1"
                };
                Jobs.Add(DisabledItem);
                foreach (EmployeeModel item in MyJobList)
                {
                    SelectListItem selectList = new SelectListItem()
                    {
                        Text = item.JobArabicName,
                        Value = item.JobId.ToString()
                    };
                    Jobs.Add(selectList);
                }
            }

            return Json(Jobs, JsonRequestBehavior.AllowGet);
                
        }
        //Remote Validation Function
        public ActionResult NationalIdValidation(double nationalID)
        {
            bool IsValid = true;
            if (nationalID.ToString().Length < 14)
            {
                IsValid = false;
            }
            return Json(IsValid, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public ActionResult SaveEmployee(EmployeeModel EmpModel)
        {
            if (ModelState.IsValid)
            {
                using (MarketEntities Market = new MarketEntities())
                {
                    Employee EmployeeToSave = new Employee();
                    JobHistory JobHistories = new JobHistory();
                    EmployeeToSave.FirstName = EmpModel.FirstName;
                    EmployeeToSave.MiddleName = EmpModel.MiddleName;
                    EmployeeToSave.LastName = EmpModel.LastName;
                    EmployeeToSave.DepartmentId = EmpModel.DepartmentId;
                    EmployeeToSave.ParentEmployeeId = EmpModel.ParentEmployeeId;
                    EmployeeToSave.NationalId = EmpModel.NationalId;
                    EmployeeToSave.MobileNumber = EmpModel.MobileNumber;
                    EmployeeToSave.EmailAddress = EmpModel.EmailAddress;
                    EmployeeToSave.NickName = EmpModel.NickName;
                    EmployeeToSave.Active = 1;
                    Market.Employees.Add(EmployeeToSave);
                    int rowAffected = Market.SaveChanges();
                    JobHistories.JobId = EmpModel.JobId;
                    JobHistories.DepartmentId = EmpModel.DepartmentId;
                    JobHistories.EmployeeId = Market.Employees.Max(id=>id.EmployeeId);
                    Market.JobHistories.Add(JobHistories);
                    rowAffected += Market.SaveChanges();
                    TempData["Msg"] = rowAffected > 0 ? "تم الحفظ بنجاح" : "لم يتم الحفظ";
                    using (AuthenticationEntities Authentication = new AuthenticationEntities())
                    {
                        // Crypt Pass
                        SHA1 s = new SHA1CryptoServiceProvider();
                        byte[] bytes = Encoding.UTF8.GetBytes(EmpModel.Password);
                        byte[] bytess = s.ComputeHash(bytes);
                        string sos = Convert.ToBase64String(bytess);
                        login EmployeeLogin = new login();
                        login lastid =  Authentication.logins.OrderByDescending(x => x.id).FirstOrDefault(); 
                        EmployeeLogin.id = lastid.id + 1;
                        EmployeeLogin.username = EmpModel.UserName;
                        EmployeeLogin.password = sos;
                        EmployeeLogin.employee_id = EmployeeToSave.EmployeeId;
                        EmployeeLogin.department = EmpModel.DepartmentId;
                        EmployeeLogin.FirstName = EmpModel.FirstName;
                        EmployeeLogin.MiddleName = EmpModel.MiddleName;
                        EmployeeLogin.LastName = EmpModel.LastName;
                        EmployeeLogin.active = 1;
                        Authentication.logins.Add(EmployeeLogin);
                        rowAffected = Authentication.SaveChanges();
                        TempData["Msg"] = rowAffected > 0 ? "تم الحفظ بنجاح" : "لم يتم الحفظ";
                    }

                    ModelState.Clear();
                    return RedirectToAction("AddEmployee", EmpModel);
                }
            }
            else
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors);
                foreach (ModelState modelstate in ViewData.ModelState.Values)
                {
                    foreach (ModelError error in modelstate.Errors)
                    {
                        TempData["ModelErrors"] += error.ErrorMessage;
                    }
                }
                return RedirectToAction("AddEmployee", EmpModel);
            }
        }
        [HttpGet]
        public ActionResult EditEmployee(int id)
        {
            EmployeeModel model = new EmployeeModel();
            using (MarketEntities db = new MarketEntities())
            {
                int? jobId = (from j in db.JobHistories
                              where j.EmployeeId == id
                              select j.JobId
             ).FirstOrDefault();
                model = (from e in db.Employees
                         join d in db.Departments on e.DepartmentId equals d.DepartmentId
                         join p in db.Employees on e.ParentEmployeeId equals p.EmployeeId
                         into u
                         from parentemp in u.DefaultIfEmpty()
                         where e.EmployeeId == id
                         select new EmployeeModel()
                         {
                             EmployeeId = e.EmployeeId,
                             EmployeeNo = e.EmployeeNo,
                             FirstName = e.FirstName,
                             MiddleName = e.MiddleName,
                             LastName = e.LastName,
                             DepartmentId = d.DepartmentId,
                             ParentEmployeeName = (parentemp.FirstName + " " + parentemp.MiddleName + " " + parentemp.LastName) ?? String.Empty,
                             ParentEmployeeId = e.ParentEmployeeId,
                             NationalId = e.NationalId,
                             MobileNumber = e.MobileNumber,
                             EmailAddress = e.EmailAddress,
                             NickName = e.NickName,
                             Active = e.Active,
                             JobId = jobId
                             
                         }).FirstOrDefault<EmployeeModel>();
            }
            model.MyDepartments = PopulateDepartments();
            model.MyJobs = GetJob(model.DepartmentId);
            ViewBag.MyParentEmployees = PopulateParentEmp();
            return PartialView(model);
        }
        [HttpPost]
        public ActionResult EditEmployee(EmployeeModel model)
        {
            if (model.EmployeeId == 0)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            using (MarketEntities Market = new MarketEntities())
            {
                ModelState.Remove("UserName");
                ModelState.Remove("Password");
                ModelState.Remove("ConfirmPassword");
                if (ModelState.IsValid)
                {
                    var modelToUpdate = Market.Employees.Find(model.EmployeeId);
                    var jobToUpdate = (from j in Market.JobHistories
                                       where j.EmployeeId == model.EmployeeId
                                       select j
                                       ).FirstOrDefault();
                    modelToUpdate.FirstName = model.FirstName;
                    modelToUpdate.MiddleName = model.MiddleName;
                    modelToUpdate.LastName = model.LastName;
                    modelToUpdate.DepartmentId = model.DepartmentId;
                    modelToUpdate.ParentEmployeeId = model.ParentEmployeeId;
                    modelToUpdate.NationalId = model.NationalId;
                    modelToUpdate.MobileNumber = model.MobileNumber;
                    modelToUpdate.EmailAddress = model.EmailAddress;
                    modelToUpdate.NickName = model.NickName;
                    modelToUpdate.Active = model.Active;
                    jobToUpdate.JobId = model.JobId;
                    TryUpdateModel(jobToUpdate);
                    TryUpdateModel(modelToUpdate);
                    int rowAffected = Market.SaveChanges();
                    TempData["Msg"] = rowAffected > 0 ? "تم الحفظ بنجاح" : "لم يتم الحفظ";
                }
                else
                {
                    //TempData["Msg"] = "لم يتم الحفظ";
                    var errors = ModelState.Values.SelectMany(v => v.Errors);
                    foreach (ModelState modelstate in ViewData.ModelState.Values)
                    {
                        foreach (ModelError error in modelstate.Errors)
                        {
                            TempData["ModelErrors"] += error.ErrorMessage;
                        }
                    }
                }
            }
            return RedirectToAction("AllEmployees", TempData["Msg"]);
        }
    }
}