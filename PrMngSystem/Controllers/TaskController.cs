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
            // Look up the role
            string roleName = "Developer";

            var roleId = (from r in db.Roles
                          where r.role_name == roleName
                          select r.roleID).FirstOrDefault();

            // Find the users in that role
            var roleUsers = (from p in db.Users

                             where p.roleID == roleId

                             select p);

            //show task depending on role
            string userName = User.Identity.Name;

            // Look up the role
            var userInfo = (from r in db.Users
                            where r.username == userName
                            select r).FirstOrDefault();

            List<object> AssigneeList = getAssigneeDev();
            ViewBag.Assignee = new SelectList(AssigneeList, "userID", "username");

            if (userInfo == null)
            {
                return RedirectToAction("Login", "User");
            }

            if (userInfo.roleID == 3)
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
            //// Look up the role
            //string roleName = "Developer";

            //var roleId = (from r in db.Roles
            //              where r.role_name == roleName
            //              select r.roleID).FirstOrDefault();

            //// Find the users in that role
            //var roleUsers = (from p in db.Users

            //                 where p.roleID == roleId

            //                 select p);

            //List<User> AssigneeList = roleUsers.ToList();


            List<object> AssigneeList = getAssigneeDev();
            ViewBag.Assignee = new SelectList(AssigneeList, "userID", "username");

            List<ProjectStatu> StatusList = db.ProjectStatus.ToList();
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

            var roleId = (from r in db.Users
                          where r.username == User.Identity.Name
                          select r.roleID).FirstOrDefault();


            List<object> AssigneeList = getAssigneeDev();
            ViewBag.Assignee = new SelectList(AssigneeList, "userID", "username");

            List<ProjectStatu> StatusList = db.ProjectStatus.ToList();
            ViewBag.Status = new SelectList(StatusList, "statusID", "status_desc");

            switch (roleId)
            {
                case 1:
                    ViewBag.ReadonlyDate = false;
                    ViewBag.ReadonlyAssignee = false;
                    break;
                case 2:
                    ViewBag.ReadonlyDate = true;
                    ViewBag.ReadonlyAssignee = false;
                    break;
                default:
                    ViewBag.ReadonlyDate = true;
                    ViewBag.ReadonlyAssignee = true;
                    break;
            }
            

            return View(tasktUpdate);
        }

        // POST: Task/EditTask/5
        [HttpPost]
        public ActionResult EditTask(Task taskEdit)
        {

            var taskUpdate = db.Tasks.SingleOrDefault(t => t.taskID == taskEdit.taskID);
            var roleId = (from r in db.Users
                          where r.username == User.Identity.Name
                          select r.roleID).FirstOrDefault();

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
                if(roleId != 3)
                {
                    taskUpdate.assignee = taskEdit.assignee;
                }

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

        [HttpPost]
        public ActionResult AssignTask(int id, string assign)
        {

            var taskUpdate = db.Tasks.SingleOrDefault(t => t.taskID == id);
            var taskAssignee = db.Users.SingleOrDefault(t => t.username == assign);

            if (!ModelState.IsValid)

                return View();

            if (taskUpdate != null)
            {
                //using (PrMngSystemDBEntities db = new PrMngSystemDBEntities())
                //{

                taskUpdate.assignee = taskAssignee.userID;

                db.SaveChanges();
                //}
            }


            return RedirectToAction("Tasks");
        }

        [HttpPost]
        public ActionResult UnassignTask(int id)
        {

            var taskUpdate = db.Tasks.SingleOrDefault(t => t.taskID == id);

            if (!ModelState.IsValid)

                return View();

            if (taskUpdate != null)
            {
                //using (PrMngSystemDBEntities db = new PrMngSystemDBEntities())
                //{

                taskUpdate.assignee = null;

                db.SaveChanges();
                //}
            }


            return RedirectToAction("Tasks");
        }

        private List<object> getAssigneeDev()
        {
            IQueryable<object> result = from u in db.Users
                                        join t in db.Tasks on u.userID equals t.assignee into ps
                                        from t in ps.DefaultIfEmpty()
                                        group t by new
                                        {
                                            t.assignee,
                                            u.username,
                                            u.roleID
                                        } into tgroup
                                        where tgroup.Count() < 3 && tgroup.Key.roleID == 3
                                        select new
                                        {
                                            userID = tgroup.Key.assignee,
                                            username = tgroup.Key.username
                                        };

            return result.ToList();
        }

    }
}