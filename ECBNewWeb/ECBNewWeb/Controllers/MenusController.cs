using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ECBNewWeb.Models;
using ECBNewWeb.DataAccess;
using System.Data.SqlClient;
using System.Configuration;
using Newtonsoft.Json;
using System.Data;

namespace ECBNewWeb.Controllers
{
    public class MenusController : Controller
    {
        List<MenuManagementModel> MenuSource;
        List<MenuManagementDeleteModel> MenuSourceForDelete;
        AuthenticationEntities db = new AuthenticationEntities();
        public ActionResult ManageMenu()
        {
            MenuManagementModel model = new MenuManagementModel();
            model.MyRoles = PopulateRoles();
            return View(model);
        }
        public JsonResult BuildMenu()
        {
            MenuSource = (from m in db.Menus
                          select new MenuManagementModel()
                          {
                              id = m.MenuId,
                              text = m.ArabicName,
                              ParentMenuId = m.ParentMenuId
                          }).ToList();

            var Menus = CreateMenus(0, MenuSource);
            return Json(Menus,JsonRequestBehavior.AllowGet);
        }
        public IEnumerable<MenuManagementModel> CreateMenus(int parentId, List<MenuManagementModel> Source)
        {
            var result = from m in Source
                         where m.ParentMenuId == parentId
                         select new MenuManagementModel()
                         {
                             id = m.id,
                             text = m.text,
                             children = CreateMenus(m.id, Source).ToList()
                         };
            return result;
        }
        public List<SelectListItem> PopulateRoles()
        {
            List<SelectListItem> Items = new List<SelectListItem>();
            using (AuthenticationEntities db = new AuthenticationEntities())
            {
                List<MenuManagementModel> MyRoles = (from r in db.Roles
                                                     select new MenuManagementModel() { RoleId= r.RoleID, RoleArabicName = r.RoleArabicName
                                                     }).ToList();
                foreach (MenuManagementModel Role in MyRoles)
                {

                    SelectListItem selectList = new SelectListItem()
                    {
                        Text = Role.RoleArabicName,
                        Value = Role.RoleId.ToString(),
                    };

                    Items.Add(selectList);
                }
            }
            return Items;
        }
        public ActionResult IsRoleHasMenus(int RoleId)
        {
            bool IsExists = true;
            int Result = 0;
            string Cmd = "Select Count(MenuRoleId) FROM MenuRoles Where RoleId = @RoleId";
            using (SqlConnection Conn = new SqlConnection(ConfigurationManager.ConnectionStrings["ECBConnectionString"].ConnectionString))
            {
                Conn.Open();
                using (SqlCommand Com = new SqlCommand(Cmd, Conn))
                {
                    Com.Parameters.AddWithValue("@RoleId", RoleId);
                    Result = (Int32)Com.ExecuteScalar();
                    if (Result > 0)
                    {
                        IsExists = false;
                    }
                }
            }
            return Json(IsExists, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public ActionResult SaveMenuRoles(int[] UndeterminedParents,int[] checked_ids,int RoleId)
        {
            int InsertedRows = 0;
            using (SqlConnection Con = new SqlConnection(ConfigurationManager.ConnectionStrings["ECBConnectionString"].ConnectionString))
            {
                Con.Open();
                using (SqlCommand Com = new SqlCommand("",Con))
                {
                    if (UndeterminedParents != null)
                    {
                        for (int i = 0; i < UndeterminedParents.Length; i++)
                        {
                            Com.CommandText = "Insert Into MenuRoles(MenuId,RoleId) Values(" + UndeterminedParents[i] + "," + RoleId + ")";
                            InsertedRows = Com.ExecuteNonQuery();
                        }
                    }
                    if (checked_ids != null)
                    {
                        for (int i = 0; i < checked_ids.Length; i++)
                        {
                            Com.CommandText = "Insert Into MenuRoles(MenuId,RoleId) Values(" + checked_ids[i] + "," + RoleId + ")";
                            InsertedRows += Com.ExecuteNonQuery();
                        }
                        if (InsertedRows > 0)
                        {
                            Session["Msg"] = "تم الحفظ بنجاح";
                        }
                        else
                        {
                            Session["Msg"] = "لم يتم الحفظ";
                        }
                    }
                    else
                    {
                        Session["Msg"] = "يجب إختيار قائمة على الأقل";
                    }
                }
            }
            return RedirectToAction("ManageMenu");
        }
        public ActionResult AllMenus()
        {
            MenuManagementDeleteModel model = new MenuManagementDeleteModel();
            model.MyRoles = PopulateRoles();
            return View(model);
        }
        public JsonResult BuildPredefinedMenu(int RoleId)
        {
            MenuSourceForDelete = (from m in db.Menus
                          join mr in db.MenuRoles on m.MenuId equals mr.MenuId
                          join r in db.Roles on mr.RoleId equals r.RoleID
                          where mr.RoleId == RoleId
                          select new MenuManagementDeleteModel()
                          {
                              id = m.MenuId,
                              text = m.ArabicName,
                              ParentMenuId = m.ParentMenuId
                          }).Distinct().ToList();

            var Menus = CreatePredefinedMenus(0, MenuSourceForDelete);
            return Json(Menus, JsonRequestBehavior.AllowGet);
        }
        public IEnumerable<MenuManagementDeleteModel> CreatePredefinedMenus(int parentId, List<MenuManagementDeleteModel> Source)
        {
            var result = from m in Source
                         where m.ParentMenuId == parentId
                         select new MenuManagementDeleteModel()
                         {
                             id = m.id,
                             text = m.text,
                             children = CreatePredefinedMenus(m.id, Source).ToList()
                         };
            return result;
        }
        [HttpPost]
        public ActionResult DeletePredefinedMenus(MenuManagementDeleteModel model)
        {
            int DeletedRowsCount = 0;
            string Command = "Delete From MenuRoles Where RoleId = @RoleId";
            using (SqlConnection Con = new SqlConnection(ConfigurationManager.ConnectionStrings["ECBConnectionString"].ConnectionString))
            {
                Con.Open();
                using (SqlCommand Cmd = new SqlCommand(Command,Con))
                {
                    Cmd.Parameters.AddWithValue("@RoleId", model.RoleId);
                    DeletedRowsCount = Cmd.ExecuteNonQuery();
                }

                if (DeletedRowsCount > 0)
                {
                    TempData["Msg"] = "تم الحذف بنجاح";
                }
                else
                {
                    TempData["Msg"] = "لم يتم الحفظ";
                }
            }
            return RedirectToAction("AllMenus", TempData["Msg"]);
        }
        public ActionResult AddRole(MenuManagementModel model)
        {
            int InsertedRows = 0;
            using (AuthenticationEntities db = new AuthenticationEntities())
            {
                Role RoleToSave = new Role();
                RoleToSave.RoleArabicName = model.RoleArabicName;
                RoleToSave.RoleEnglishName = model.RoleEnglishName;
                RoleToSave.RoleDescription = model.RoleDescription;
                db.Roles.Add(RoleToSave);
                InsertedRows = db.SaveChanges();
            }
            if (InsertedRows > 0)
            {
                TempData["Msg2"] = "تم الحفظ بنجاح";
            }
            else
            {
                TempData["Msg2"] = "لم يتم الحفظ";
            }
            return RedirectToAction("ManageMenu");
        }
    }
}