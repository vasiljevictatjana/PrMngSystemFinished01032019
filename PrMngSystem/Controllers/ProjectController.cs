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
            var userInfo = (from r in db.Users
                          where r.username == User.Identity.Name
                          select r).FirstOrDefault();

            //manager see only his projects
            if(userInfo.roleID == 2)
            {
                return View(db.Projects.Where(p => p.manage == userInfo.userID).ToList());
            }

            return View(db.Projects.ToList());
        }

        // GET: Home/Create
        public ActionResult CreateProject()
        {
            //manager can not assign project to someone else
            var roleId = (from r in db.Users
                          where r.username == User.Identity.Name
                          select r.roleID).FirstOrDefault();

            switch (roleId)
            {
                case 1:
                    ViewBag.Manage = true;
                    break;
                case 2:
                    ViewBag.Manage = false;
                    break;
                default:
                    ViewBag.Manage = false;
                    break;
            }

            // Find the users in that role
            var roleUsers = (from p in db.Users

                             where p.roleID == 3

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
                var userInfo = db.Users.SingleOrDefault(u => u.username == User.Identity.Name);

                if (userInfo.roleID == 2)
                {
                    newProject.manage = userInfo.userID;
                }
                
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

            //manager can not assign project to someone else
            var roleId = (from r in db.Users
                          where r.username == User.Identity.Name
                          select r.roleID).FirstOrDefault();

            switch (roleId)
            {
                case 1:
                    ViewBag.Manage = true;
                    break;
                case 2:
                    ViewBag.Manage = false;
                    break;
                default:
                    ViewBag.Manage = false;
                    break;
            }

            // Find the users in that role
            var roleUsers = (from p in db.Users

                             where p.roleID == 3

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
                    var userInfo = db.Users.SingleOrDefault(u => u.username == User.Identity.Name);

                    projectUpdate.project_code = projectEdit.project_code;
                    projectUpdate.project_name = projectEdit.project_name;
                    if (userInfo.roleID == 2)
                    {
                        projectUpdate.manage = userInfo.userID;
                    }
                    else
                    {
                        projectUpdate.manage = projectEdit.manage;
                    }

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