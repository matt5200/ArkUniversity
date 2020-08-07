using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using WebApplication1.Models;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Db;

namespace WebApplication1.Controllers
{
    public class HomeController : Controller
    {

        private readonly IUserManager userManager;
        private readonly CookieAuthenticationOptions _cookieAuthenticationOptions;


        public HomeController(IUserManager userManager, IOptionsMonitor<CookieAuthenticationOptions> cookieAuthenticationOptions)
        {
            this.userManager = userManager;
            _cookieAuthenticationOptions = cookieAuthenticationOptions.Get(CookieAuthenticationDefaults.AuthenticationScheme);
            var loginPath = _cookieAuthenticationOptions.LoginPath;
        }

        [Authorize]
        public IActionResult Enroll(int classId)
        {
            if (classId != 0)
            {
                var user = JsonConvert.DeserializeObject<Models.UserModel>(HttpContext.Session.GetString("User"));
                var dbInstance = DatabaseAccessor.instance;
                var classList = dbInstance.UserClass.Where(t => t.UserId == user.Id).Where(t => t.ClassId == classId).ToList();
                if (classList.Count() == 0)
                {
                    return View("ThankYou");
                }
                else
                {
                    return View("Enrollment succesfull");
                }
            }
            return View();
        }


        public IActionResult Index()
        {
            return View();
        }

        [Authorize]
        public ActionResult YourClasses ()
        {
            var user = JsonConvert.DeserializeObject<Models.UserModel>(HttpContext.Session.GetString("User"));


            var dbInstance = DatabaseAccessor.instance;
            
            var classList = dbInstance.UserClass.Where(t => t.UserId == user.Id).ToList();

            var classes = dbInstance.Class.Where(t => classList.Find(c => c.ClassId == t.ClassId) != null);

            return View(classes);
        }

        public IActionResult About()
        {
            return View();
        }

        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public ActionResult ClassList()
        {
            return View();
        }


        public ActionResult Login()
        {
            ViewData["ReturnUrl"] = Request.Query["returnUrl"];
            return View();
        }

        [HttpPost]
        public ActionResult Login(Login loginModel, string returnUrl)
        {
            if (ModelState.IsValid)
            {
                var user = userManager.LogIn(loginModel.UserName, loginModel.Password);

                if (user == null)
                {
                    ModelState.AddModelError("", "User name and password do not match.");
                }
                else
                {
                    var json = JsonConvert.SerializeObject(new WebApplication1.Models.UserModel
                    {
                        Id = user.Id,
                        Name = user.Name
                    });

                    HttpContext.Session.SetString("User", json);

                    var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.Name),
                    new Claim(ClaimTypes.Role, "User"),
                };

                    var claimsIdentity = new ClaimsIdentity(claims,
                        CookieAuthenticationDefaults.AuthenticationScheme);

                    var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

                    var authProperties = new AuthenticationProperties
                    {
                        AllowRefresh = false,
                        // Refreshing the authentication session should be allowed.

                        ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(10),
                        // The time at which the authentication ticket expires. A 
                        // value set here overrides the ExpireTimeSpan option of 
                        // CookieAuthenticationOptions set with AddCookie.

                        IsPersistent = false,
                        // Whether the authentication session is persisted across 
                        // multiple requests. When used with cookies, controls
                        // whether the cookie's lifetime is absolute (matching the
                        // lifetime of the authentication ticket) or session-based.

                        IssuedUtc = DateTimeOffset.UtcNow,
                        // The time at which the authentication ticket was issued.
                    };

                    HttpContext.SignInAsync(
                        CookieAuthenticationDefaults.AuthenticationScheme,
                        claimsPrincipal,
                        authProperties).Wait();

                    return Redirect(returnUrl ?? "~/");
                }
            }

            ViewData["ReturnUrl"] = returnUrl;

            return View(loginModel);
        }

        public ActionResult LogOff()
        {
            HttpContext.Session.Remove("User");

            HttpContext.SignOutAsync(
            CookieAuthenticationDefaults.AuthenticationScheme);

            return Redirect("~/");
        }

        [HttpPost]
        public ActionResult Register(RegisterModel registerModel)
        {
            if (ModelState.IsValid)
            {
                userManager.Register(registerModel.UserName, registerModel.Password);

                return Redirect("~/");
            }
            else
            {
                return View();
            }
        }

        [HttpGet]
        public ActionResult Register()
        {
            return View();
        }

    }
}
