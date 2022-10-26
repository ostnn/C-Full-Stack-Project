using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using HobbyHub.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using System.Dynamic;



namespace HobbyHub.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        private MyContext _context;

        public HomeController(ILogger<HomeController> logger, MyContext context)
        {
            _logger = logger;
            _context = context;
        }

        public IActionResult Index()
        {
            if (HttpContext.Session.GetInt32("UserId") != null)
            {
                return RedirectToAction("Dashboard");
            }
            return View();
        }
        public IActionResult Privacy()
        {
            if (HttpContext.Session.GetInt32("UserId") != null)
            {
                return RedirectToAction("Dashboard");
            }
            return View();
        }

        [HttpPost("users/add")]
        public IActionResult CreateUser(User newUser)
        {
            if (ModelState.IsValid)
            {
                if (_context.Users.Any(s => s.UserName == newUser.UserName))
                {
                    ModelState.AddModelError("Username", "Username already in use!");
                    return View("Index");
                }
                PasswordHasher<User> Hasher = new PasswordHasher<User>();
                newUser.Password = Hasher.HashPassword(newUser, newUser.Password);
                _context.Add(newUser);
                _context.SaveChanges();
                HttpContext.Session.SetInt32("UserId", newUser.UserId);
                return RedirectToAction("Dashboard");
            }
            else
            {
                return View("Index");
            }
        }

        [HttpPost("users/login")]
        public IActionResult Login(LoginUser loginUser)
        {
            if (ModelState.IsValid)
            {
                User userInDb = _context.Users.FirstOrDefault(d => d.UserName == loginUser.LoginUserName);
                if (userInDb == null)
                {
                    ModelState.AddModelError("LoginUserName", "Invalid Email/Password!");
                    return View("Index");
                }
                PasswordHasher<LoginUser> hasher = new PasswordHasher<LoginUser>();

                var result = hasher.VerifyHashedPassword(loginUser, userInDb.Password, loginUser.LoginPassword);

                if (result == 0)
                {
                    ModelState.AddModelError("LoginPassword", "Invalid Email/Password!");
                    return View("Index");
                }
                HttpContext.Session.SetInt32("UserId", userInDb.UserId);
                return RedirectToAction("Dashboard");
            }
            else
            {
                return View("Index");
            }
        }

        [HttpGet("Dashboard")]
        public IActionResult Dashboard()
        {
            if (HttpContext.Session.GetInt32("UserId") == null)
            {
                return RedirectToAction("Index");
            }
            else
            {
                dynamic model = new ExpandoObject();
                model.LoggedInUser = _context.Users.FirstOrDefault(a => a.UserId == (int)HttpContext.Session.GetInt32("UserId"));
                model.Hobbies = _context.Hobbies.Include(a => a.Hobbyist).Include(a => a.TheEnthusiasts).ToList();
                return View(model);
            }
        }

        [HttpGet("NewHobby")]
        public IActionResult NewHobby()
        {
            if (HttpContext.Session.GetInt32("UserId") == null)
            {
                return RedirectToAction("Index");
            }
            else
            {
                ViewBag.LoggedInUserId = (int)HttpContext.Session.GetInt32("UserId");
                return View();
            }
        }

        [HttpPost("SubmitHobby")]
        public IActionResult SubmitHobby(Hobby newHobby)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors);
            foreach (var error in errors)
            {
                Console.WriteLine(error.ErrorMessage);
            }
            if (ModelState.IsValid)
            {
                newHobby.UserId = (int)HttpContext.Session.GetInt32("UserId");
                _context.Hobbies.Add(newHobby);
                _context.SaveChanges();
                return RedirectToAction("Dashboard");
            }
            else
            {
                ViewBag.LoggedInUserId = (int)HttpContext.Session.GetInt32("UserId");
                return View("NewHobby");
            }
        }

        [HttpGet("ShowHobby/{HobbyId}")]
        public IActionResult ShowHobby(int HobbyId)
        {
            Hobby hobby = _context.Hobbies.Include(a => a.Hobbyist).Include(h => h.TheEnthusiasts).FirstOrDefault(a => a.HobbyId == HobbyId);
            List<int> userIds = _context.Enthusiasts.Where(g => g.HobbyId == HobbyId).Select(g => g.UserId).ToList();
            hobby.Hobbyist = _context.Users.FirstOrDefault(u => u.UserId == hobby.UserId);
            dynamic model = new ExpandoObject();
            model.Enthusiasts = _context.Users.Where(u => userIds.Contains(u.UserId));
            model.LoggedInUser = _context.Users.FirstOrDefault(a => a.UserId == (int)HttpContext.Session.GetInt32("UserId"));
            model.hobby = hobby;

            return View(model);
        }

        [HttpGet("Add/{HobbyId}")]
        public IActionResult Add(int HobbyId)
        {
            Enthusiast enthusiast = new Enthusiast();
            enthusiast.UserId = (int)HttpContext.Session.GetInt32("UserId");
            enthusiast.HobbyId = HobbyId;
            _context.Enthusiasts.Add(enthusiast);
            _context.SaveChanges();
            return RedirectToAction("Dashboard");
        }

        [HttpGet("Logout")]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index");
        }


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
