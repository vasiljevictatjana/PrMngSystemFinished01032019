using PrMngSystem.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace PrMngSystem.Controllers
{
    public class TaskController : Controller
    {
        private PrMngSystemDBEntities db = new PrMngSystemDBEntities();

        // GET: Task
        public ActionResult Tasks()
        {
            string userName = User.Identity.Name;

            // Look up the role
            var userInfo = (from r in db.Users
                          where r.username == userName
                          select r).FirstOrDefault();

            if(userInfo.roleID == 3)
            {
                // Find the users in that role
                var userTasks = (from t in db.Tasks

                                 where t.assignee == userInfo.userID

                                 select t);

                return View(userTasks.ToList());
            }
            else
            {
                return View(db.Tasks.ToList());
            }
            
        }

        // GET: Task/CreateTask
        public ActionResult CreateTask()
        {
            // Look up the role
            string roleName = "Developer";

            var roleId = (from r in db.Roles
                          where r.role_name == roleName
                          select r.roleID).FirstOrDefault();

            // Find the users in that role
            var roleUsers = (from p in db.Users

                             where p.roleID == roleId

                             select p);

            List<User> AssigneeList = roleUsers.ToList();

            List<ProjectStatu> StatusList = db.ProjectStatus.ToList();

            ViewBag.Assignee = new SelectList(AssigneeList, "userID", "username");
            ViewBag.Status = new SelectList(StatusList, "statusID", "status_desc");

            return View();
        }

        // POST: Task/CreateTask/1
        [HttpPost]
        public ActionResult CreateTask(int id, Task newTask)
        {

            if (!ModelState.IsValid)
                return View(newTask);

            newTask.projectID = id;

            using (PrMngSystemDBEntities db = new PrMngSystemDBEntities())
            {
                db.Tasks.Add(newTask);
                db.SaveChanges();
            }

            //add Task to list Tasks in Project 
            var project = db.Projects.Find(id);
            project.Tasks.Add(newTask);

            return RedirectToAction("Tasks");

        }

        // GET: Task/EditTask/5
        public ActionResult EditTask(int id)
        {
            var tasktUpdate = (from t in db.Tasks

                                 where t.taskID == id

                                 select t).FirstOrDefault();

            // Look up the role
            string roleName = "Developer";

            var roleId = (from r in db.Roles
                          where r.role_name == roleName
                          select r.roleID).FirstOrDefault();

            // Find the users in that role
            var roleUsers = (from p in db.Users

                             where p.roleID == roleId

                             select p);

            List<User> AssigneeList = roleUsers.ToList();

            List<ProjectStatu> StatusList = db.ProjectStatus.ToList();

            ViewBag.Assignee = new SelectList(AssigneeList, "userID", "username");
            ViewBag.Status = new SelectList(StatusList, "statusID", "status_desc");

            return View(tasktUpdate);
        }

        // POST: Task/EditTask/5
        [HttpPost]
        public ActionResult EditTask(Task taskEdit)
        {

            var taskUpdate = db.Tasks.SingleOrDefault(t => t.taskID == taskEdit.taskID);

            if (!ModelState.IsValid)

                return View(taskEdit);

            if (taskUpdate != null)
            {
                //using (PrMngSystemDBEntities db = new PrMngSystemDBEntities())
                //{

                taskUpdate.progress = taskEdit.progress;
                taskUpdate.deadline = taskEdit.deadline;
                taskUpdate.description = taskEdit.description;
                taskUpdate.status = taskEdit.status;
                taskUpdate.assignee = taskEdit.assignee;

                db.SaveChanges();
                //}
            }


            return RedirectToAction("Tasks");
        }

        // GET: Task/DeleteTask/5
        public ActionResult DeleteTask(int id)
        {
            var taskDelete = (from t in db.Tasks

                                 where t.taskID == id

                                 select t).FirstOrDefault();

            return View(taskDelete);
        }


        // POST: Task/DeleteTask/5
        [HttpPost, ActionName("DeleteTask")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteTaskConfirmed(int id)
        {
            if (!ModelState.IsValid)
                return View();

            using (PrMngSystemDBEntities db = new PrMngSystemDBEntities())
            {

                var taskDelete = db.Tasks.Where(t => t.taskID == id).FirstOrDefault();

                //remove Task from Project list of Tasks
                var project = db.Projects.SingleOrDefault(p => p.projectID == taskDelete.projectID);
                project.Tasks.Remove(taskDelete);

                //delete task
                db.Tasks.Remove(taskDelete);

                db.SaveChanges();
 
            }

            return RedirectToAction("Tasks");

        }
    }
}