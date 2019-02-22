using PrMngSystem.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace PrMngSystem.Controllers
{
    public class ProjectController : Controller
    {
        private PrMngSystemDBEntities db = new PrMngSystemDBEntities();

        [Authorize]
        public ActionResult Projects()
        {

            return View(db.Projects.ToList());
        }

        // GET: Home/Create
        public ActionResult CreateProject()
        {
            // Look up the role
            string roleName = "Project Manager";
            var roleId = (from r in db.Roles
                        where r.role_name == roleName
                        select r.roleID).FirstOrDefault();

            // Find the users in that role
            var roleUsers = (from p in db.Users

                             where p.roleID == roleId

                             select p);

            List<User> ManagerList = roleUsers.ToList();

            ViewBag.Managers = new SelectList(ManagerList, "userID", "username");

            return View();
        }

        // POST: Home/Create
        [HttpPost]
        public ActionResult CreateProject(Project newProject)
        {

            if (!ModelState.IsValid)
                return View(newProject);

            using (PrMngSystemDBEntities db = new PrMngSystemDBEntities())
            {
                db.Projects.Add(newProject);
                db.SaveChanges();
            }

            return RedirectToAction("Projects");

        }

        // GET: Home/Edit/5
        public ActionResult EditProject(int id)
        {
            var projectUpdate = (from p in db.Projects

                                 where p.projectID == id

                                 select p).FirstOrDefault();

            // Look up the role
            string roleName = "Project Manager";
            var roleId = (from r in db.Roles
                          where r.role_name == roleName
                          select r.roleID).FirstOrDefault();

            // Find the users in that role
            var roleUsers = (from p in db.Users

                             where p.roleID == roleId

                             select p);

            List<User> ManagerList = roleUsers.ToList();

            ViewBag.Managers = new SelectList(ManagerList, "userID", "username");

            return View(projectUpdate);
        }

        // POST: Home/Edit/5
        [HttpPost]
        public ActionResult EditProject(Project projectEdit)
        {

            var projectUpdate = db.Projects.SingleOrDefault(p => p.projectID == projectEdit.projectID);

            if (!ModelState.IsValid)

                return View(projectEdit);

            if (projectUpdate != null)
            {
                //using (PrMngSystemDBEntities db = new PrMngSystemDBEntities())
                //{
                
                    projectUpdate.project_code = projectEdit.project_code;
                    projectUpdate.project_name = projectEdit.project_name;
                    projectUpdate.manage = projectEdit.manage;
                    
                    db.SaveChanges();
                //}
            }
                

            return RedirectToAction("Projects");
        }

        // GET: Home/Delete/5
        public ActionResult DeleteProject(int id)
        {
            var projectDelete = (from p in db.Projects

                                 where p.projectID == id

                                 select p).FirstOrDefault();

            return View(projectDelete);
        }


        // POST: Home/Delete/5
        [HttpPost, ActionName("DeleteProject")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            if (!ModelState.IsValid)
                return View();

            using (PrMngSystemDBEntities db = new PrMngSystemDBEntities())
            {
            //var projectDelete = (from p in db.Projects

            //                     where p.projectID == id

            //                     select p).FirstOrDefault();

                var projectDelete = db.Projects.Where(p => p.projectID == id).FirstOrDefault();

                //delete all tasks of project
                var listTasks = projectDelete.Tasks.ToList();
                listTasks.ForEach(x => db.Tasks.Remove(x));

                //delete project
                db.Projects.Remove(projectDelete);

                db.SaveChanges();
            }

            return RedirectToAction("Projects");

        }


    }
}