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
        [Authorize]
        public ActionResult Tasks()
        {
            // Look up the role, show task depending on role
            var userInfo = (from r in db.Users
                            where r.username == User.Identity.Name
                            select r).FirstOrDefault();

            if (userInfo == null)
            {
                return RedirectToAction("Login", "User");
            }

            List<object> AssigneeList = getAssigneeDev();
            ViewBag.Assignee = new SelectList(AssigneeList, "userID", "username");

            switch (userInfo.roleID)
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

            if (userInfo == null)
            {
                return RedirectToAction("Login", "User");
            }

            ViewBag.RoleID = userInfo.roleID;

            switch (userInfo.roleID)
            {
                case 2:
                  var result =  from t in db.Tasks
                                join p in db.Projects on t.projectID equals p.projectID
                                where p.manage == userInfo.userID
                                select t;

                    return View(result.ToList());
                case 3:
                    var countAssignTasks = db.Tasks.Where(t => t.assignee == userInfo.userID).Count();

                    if (countAssignTasks > 2)
                    {
                        return View(db.Tasks.Where(t => t.assignee == userInfo.userID).ToList());
                    }
                    else
                    {
                        return View(db.Tasks.Where(t => t.assignee == userInfo.userID || t.assignee == null).ToList());
                    }
                default:
                    return View(db.Tasks.ToList());
            }
        }

        // GET: Task/TasksProject/1
        public ActionResult TasksProject(int id)
        {

            // Look up the role, show task depending on role
            var userInfo = (from r in db.Users
                            where r.username == User.Identity.Name
                            select r).FirstOrDefault();

            if (userInfo == null)
            {
                return RedirectToAction("Login", "User");
            }

            List<object> AssigneeList = getAssigneeDev();
            ViewBag.Assignee = new SelectList(AssigneeList, "userID", "username");

            switch (userInfo.roleID)
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

            ViewBag.RoleID = userInfo.roleID;

            var project = (from p in db.Projects

                           where p.projectID == id

                           select p).First();
        
            return View(project.Tasks.ToList());

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

            using (PrMngSystemDBEntities db = new PrMngSystemDBEntities())
            {
                newTask.projectID = id;

                db.Tasks.Add(newTask);
                db.SaveChanges();

                var project = db.Projects.Find(id);
                //add Task to list Tasks in Project 
                //var project = db.Projects.Find(id);
                project.Tasks.Add(newTask);
            }

            UpdateProjectProgress(newTask, "create");

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
            var userInfo = (from r in db.Users
                          where r.username == User.Identity.Name
                          select r).FirstOrDefault();

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
                if (userInfo.roleID == 3)
                {
                    taskUpdate.assignee = userInfo.userID; 
                }
                else
                {
                    taskUpdate.assignee = taskEdit.assignee;
                }

                db.SaveChanges();

                UpdateProjectProgress(taskEdit, "edit");

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

                UpdateProjectProgress(taskDelete, "delete");
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

            if (taskUpdate != null && taskAssignee != null)
            {
                var countAssignTasks = db.Tasks.Where(t => t.assignee == taskAssignee.userID).Count();

                if (countAssignTasks > 2)
                {
                    return RedirectToAction("Tasks");
                }
            
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

            var userInfo = (from r in db.Users
                            where r.username == User.Identity.Name
                            select r).FirstOrDefault();

            IQueryable<object> result;

            if (userInfo.roleID == 2)
            {
                //get all developers with less than 3 tasks assigneed and this manager himself
                result = (from u in db.Users
                        join t in db.Tasks on u.userID equals t.assignee into ps
                        from t in ps.DefaultIfEmpty()
                        group t by new
                        {
                            t.assignee,
                            u.username,
                            u.userID,
                            u.roleID
                        } into tgroup
                        where tgroup.Count() < 3 && tgroup.Key.roleID == 3
                        select new
                        {
                            userID = tgroup.Key.userID,
                            username = tgroup.Key.username
                        }
                        ).Union
                        (from u in db.Users
                         join t in db.Tasks on u.userID equals t.assignee into ps
                         from t in ps.DefaultIfEmpty()
                         group t by new
                         {
                             t.assignee,
                             u.username,
                             u.userID,
                             u.roleID
                         } into tgroup
                         where tgroup.Key.username == User.Identity.Name
                         select new
                         {
                             userID = tgroup.Key.userID,
                             username = tgroup.Key.username
                         });
            }
            else
            {
                //get all developers with less than 3 tasks assignesd
                result = (from u in db.Users
                          join t in db.Tasks on u.userID equals t.assignee into ps
                          from t in ps.DefaultIfEmpty()
                          group t by new
                          {
                              t.assignee,
                              u.userID,
                              u.username,
                              u.roleID
                          } into tgroup
                          where tgroup.Count() < 3 && tgroup.Key.roleID == 3
                          select new
                          {
                              userID = tgroup.Key.userID,
                              username = tgroup.Key.username
                          }
                        ).Union
                        (from u in db.Users
                         join t in db.Tasks on u.userID equals t.assignee into ps
                         from t in ps.DefaultIfEmpty()
                         group t by new
                         {
                             t.assignee,
                             u.username,
                             u.userID,
                             u.roleID
                         } into tgroup
                         where tgroup.Key.roleID == 2
                         select new
                         {
                             userID = tgroup.Key.userID,
                             username = tgroup.Key.username
                         });
            }
            

            return result.ToList();
        }

        private void UpdateProjectProgress(Task task, string action)
        {
            try
            {

                //edit Project progress
                //using (PrMngSystemDBEntities db = new PrMngSystemDBEntities())
                //{
                var project = db.Projects.SingleOrDefault(p => p.projectID == task.projectID);

                if (action == "delete")
                {
                    if (project.Tasks.Count() != 0)
                    {
                        var sumProjectProgress = project.progress * (project.Tasks.Count() + 1); //before deleting one task
                        var averageProjectProgress = (sumProjectProgress - task.progress) / project.Tasks.Count();
                        project.progress = Math.Round((decimal)averageProjectProgress, 2);
                    }
                    else
                    {
                        project.progress = 0;
                    }

                }
                else if (action == "edit")
                {
                    var countTasks = 0;
                    decimal? sumProgressTasks = 0;
                    decimal? averageProjectProgress = 0;

                    foreach (var projectTask in project.Tasks)
                    {
                        sumProgressTasks += projectTask.progress;
                        countTasks++;
                    }

                    if (countTasks != 0)
                    {
                        averageProjectProgress = (decimal)sumProgressTasks / countTasks;
                    }

                    project.progress = Math.Round((decimal)averageProjectProgress, 2);
                }
                else //action == "create"
                {
                    if (project.Tasks.Count() == 1)
                    {
                        project.progress = task.progress;
                    }
                    else
                    {
                        var countTasks = 0;
                        decimal? sumProgressTasks = 0;
                        decimal? averageProjectProgress = 0;

                        foreach (var projectTask in project.Tasks)
                        {
                            sumProgressTasks += projectTask.progress;
                            countTasks++;
                        }

                        if (countTasks != 0)
                        {
                            averageProjectProgress = (decimal)sumProgressTasks / countTasks;
                        }

                        project.progress = Math.Round((decimal)averageProjectProgress, 2);
                    }

                }

                db.SaveChanges();
                //}}
            }
            catch (Exception e)
            {

            }
        }


    }
}