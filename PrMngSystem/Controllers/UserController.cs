using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PrMngSystem.Models;
using System.Web.Security;

namespace PrMngSystem.Controllers
{
    public class UserController : Controller
    {
        // GET: User
        public ActionResult Index()
        {
            return View();
        }

        //Registration Action
        [HttpGet]
        public ActionResult Registration()
        {
            return View();
        }

        //Registration POST Action
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Registration(User user)
        {
            bool status = false;
            string message = "";

            //Model Validation
            if (ModelState.IsValid)
            {

                #region//Email already exist
                var isEmailExist = IsEmailExist(user.email);
                if (isEmailExist)
                {
                    ModelState.AddModelError("EmailExist", "Email already exist");
                    return View(user);
                }
                #endregion

                #region //Username already exist
                var isUsernameExist = IsUsernameExist(user.username);
                if (isUsernameExist)
                {
                    ModelState.AddModelError("UsernameExist", "Username already exist");
                    return View(user);
                }
                #endregion

                #region //Passwor Hashing
                user.password = Crypto.Hash(user.password);
                user.confirmPassword = Crypto.Hash(user.confirmPassword); //we will compare this two values
                #endregion

                user.roleID = 3;
                #region //Save data to db
                using (PrMngSystemDBEntities db = new PrMngSystemDBEntities())
                {
                    db.Users.Add(user);
                    db.SaveChanges();

                    message = "Registration successful.";
                    status = true;
                }
                #endregion
            }
            else
            {
                message = "Invalid Request";
            }

            ViewBag.Message = message;
            ViewBag.Status = status;

            return View(user);
        }

        //Login
        [HttpGet]
        public ActionResult Login()
        {
            return View();
        }

        //Login POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(UserLogin login, string returnUrl="")
        {
            string message = "";
            using(PrMngSystemDBEntities db = new PrMngSystemDBEntities())
            {
                var v = db.Users.Where(u => u.username == login.username).FirstOrDefault();
                if(v != null)
                {
                    if(string.Compare(v.password, Crypto.Hash(login.password)) == 0) //check two hashed passwords this is okej when is zero
                    {
                        //if user selected RememberMe
                        int timeout = login.rememberMe ? 525600 : 20; // 525600 min is one year, otherwise 20 min
                        var ticket = new FormsAuthenticationTicket(login.username, login.rememberMe, timeout); //cookies
                        string encrypted = FormsAuthentication.Encrypt(ticket);
                        var cookie = new HttpCookie(FormsAuthentication.FormsCookieName, encrypted);
                        cookie.Expires = DateTime.Now.AddMinutes(timeout);
                        cookie.HttpOnly = true;
                        Response.Cookies.Add(cookie);

                        if (Url.IsLocalUrl(returnUrl))
                        {
                            return Redirect(returnUrl);
                        }
                        else
                        {
                            return RedirectToAction("Projects", "Project");
                        }
                    }
                    else
                    {
                        message = "Invalid credential provided";
                    }
                }
                else
                {
                    message = "Invalid credential provided";
                }


            }

            ViewBag.Message = message;
            return View();
        }

        //Logout
        [Authorize]
        //[HttpPost]
        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            return RedirectToAction("Login", "User");
        }


        [NonAction]
        public bool IsEmailExist(string email)
        {
            using(PrMngSystemDBEntities db = new PrMngSystemDBEntities())
            {
                var v = db.Users.Where(u => u.email == email).FirstOrDefault();
                return v == null ? false : true;
            }
        }

        [NonAction]
        public bool IsUsernameExist(string username)
        {
            using (PrMngSystemDBEntities db = new PrMngSystemDBEntities())
            {
                var v = db.Users.Where(u => u.username == username).FirstOrDefault();
                return v == null ? false : true;
            }
        }
    }
}