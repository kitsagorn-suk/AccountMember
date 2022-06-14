using account.Models;
using account.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using Newtonsoft.Json;

namespace account.Controllers
{
    public class LoginController : Controller
    {
        // GET: Login
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Login(LoginModel login)
        {
            if (ModelState.IsValid)
            {
                accountEntities db = new accountEntities();
                var _pass = Base64Encode(login.Password);
                var user = (from userlist in db.system_user
                            where userlist.username == login.UserName && userlist.password == _pass
                            select new
                            {
                                userlist.id,
                                userlist.username
                            }).ToList();
                if (user.FirstOrDefault() != null)
                {
                    Session["UserName"] = user.FirstOrDefault().username;
                    Session["UserID"] = user.FirstOrDefault().id;
                    return Redirect("/Quotation/ListQuotation?id="+ user.FirstOrDefault().id);
                }
                else
                {
                    ModelState.AddModelError("", "Invalid login credentials.");
                }
            }
            return View(login);
        }

        public ActionResult LogOut()
        {
            HttpCookie cookie = new HttpCookie("account", "");
            cookie.Expires = DateTime.Now.AddYears(-1);
            Response.Cookies.Add(cookie);

            FormsAuthentication.SignOut();
            return RedirectToAction("Login", "Login", null);
        }

        public static string Base64Encode(string plainText)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return System.Convert.ToBase64String(plainTextBytes);
        }
    }
}