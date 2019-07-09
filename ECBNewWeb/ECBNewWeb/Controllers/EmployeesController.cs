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

namespace ECBNewWeb.Controllers
{
    public class EmployeesController : Controller
    {
        private CustomMembershipUser UserInfo;
        // GET: Employees
        public ActionResult AddEmployee()
        {
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
            if (deptid == 1)
            {
                Jobs = new List<SelectListItem>()
                {   new SelectListItem() { Text = "" , Value = "-1" ,  Selected = true },
                    new SelectListItem() { Text="مشرف موقع" , Value = "مشرف موقع"},
                    new SelectListItem() { Text = "مساعد مشرف" , Value = "مساعد مشرف"}
                };
            }
            else if (deptid == 2)
            {
                Jobs = new List<SelectListItem>()
                {
                    new SelectListItem() { Text = "" , Value = "-1" ,  Selected = true },
                    new SelectListItem() { Text="مبرمج" , Value = "مبرمج"},
                    new SelectListItem() { Text = "مساعد مبرمج" , Value = "مساعد مبرمج"}
                };
            }
            return Jobs;
        }
        // get job by department Json
        public JsonResult GetJobList(int deptid)
        {
            List<SelectListItem> Jobs = new List<SelectListItem>();
            if (deptid == 1)
            {
                Jobs = new List<SelectListItem>()
                {   new SelectListItem() { Text = "" , Value = "-1" ,  Selected = true },
                    new SelectListItem() { Text="مشرف موقع" , Value = "مشرف موقع"},
                    new SelectListItem() { Text = "مساعد مشرف" , Value = "مساعد مشرف"}
                };
            }
            else if(deptid == 2)
                {
                Jobs = new List<SelectListItem>()
                {
                    new SelectListItem() { Text = "" , Value = "-1" ,  Selected = true },
                    new SelectListItem() { Text="مبرمج" , Value = "مبرمج"},
                    new SelectListItem() { Text = "مساعد مبرمج" , Value = "مساعد مبرمج"}
                };
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
                    EmployeeToSave.FirstName = EmpModel.FirstName;
                    EmployeeToSave.MiddleName = EmpModel.MiddleName;
                    EmployeeToSave.LastName = EmpModel.LastName;
                    EmployeeToSave.DepartmentId = EmpModel.DepartmentId;
                    EmployeeToSave.ParentEmployeeId = EmpModel.ParentEmployeeId;
                    EmployeeToSave.NationalId = EmpModel.NationalId;
                    EmployeeToSave.MobileNumber = EmpModel.MobileNumber;
                    EmployeeToSave.EmailAddress = EmpModel.EmailAddress;
                    EmployeeToSave.NickName = EmpModel.NickName;
                    EmployeeToSave.job = EmpModel.job;
                    EmployeeToSave.Active = 1;
                    Market.Employees.Add(EmployeeToSave);
                    int rowAffected = Market.SaveChanges();
                    TempData["Msg"] = rowAffected > 0 ? "تم الحفظ بنجاح" : "لم يتم الحفظ";
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

        public ActionResult EditEmployee(int id)
        {
            EmployeeModel model = new EmployeeModel();
            using (MarketEntities db = new MarketEntities())
            {
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
                             ParentEmployeeId = parentemp.ParentEmployeeId,
                             NationalId = e.NationalId,
                             MobileNumber = e.MobileNumber,
                             EmailAddress = e.EmailAddress,
                             NickName = e.NickName,
                             Active = e.Active,
                             job = e.job
                             
                         }).FirstOrDefault<EmployeeModel>();
            }
            model.MyDepartments = PopulateDepartments();
            model.MyParentEmployees = PopulateParentEmp();
            ViewBag.joblist = GetJob(model.DepartmentId);
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
                if (ModelState.IsValid)
                {
                    var modelToUpdate = Market.Employees.Find(model.EmployeeId);
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
                    modelToUpdate.job = model.job;
                    TryUpdateModel(modelToUpdate);
                    int rowAffected = Market.SaveChanges();
                    TempData["Msg"] = rowAffected > 0 ? "تم الحفظ بنجاح" : "لم يتم الحفظ";
                }
                else
                {
                    TempData["Msg"] = "لم يتم الحفظ";
                }
            }
            return RedirectToAction("AllEmployees", TempData["Msg"]);
        }
    }
}