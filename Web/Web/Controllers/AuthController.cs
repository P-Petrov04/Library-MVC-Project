using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Common.Repositories;
using Web.ViewModels.Auth;
using Common.Entities;

namespace Web.Controllers
{
    [Route("Auth")]
    public class AuthController : Controller
    {
        private readonly BaseRepository<User> _usersRepo;

        public AuthController(BaseRepository<User> usersRepo)
        {
            _usersRepo = usersRepo;
        }

        // GET: Login Page
        [HttpGet("Login")]
        public IActionResult Login(string url)
        {
            if (HttpContext.Session.GetString("loggedUserId") != null)
            {
                return RedirectToAction("Index","Admin");
            }
            ViewData["url"] = url;
            return View();
        }

        // POST: Login User
        [HttpPost("Login")]
        public IActionResult Login(LoginVM model)
        {
            if (HttpContext.Session.GetString("loggedUserId") != null)
            {
                return RedirectToAction("Index", "Admin");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var loggedUser = _usersRepo.FirstOrDefault(u =>
                u.Email == model.Email && u.Password == model.Password);

            if (loggedUser == null)
            {
                ModelState.AddModelError("authError", "Invalid login attempt.");
                return View(model);
            }

            HttpContext.Session.SetString("loggedUserId", loggedUser.Id.ToString());
            HttpContext.Session.SetString("UserEmail", loggedUser.Email);
            HttpContext.Session.SetString("UserName", loggedUser.Name);
            HttpContext.Session.SetInt32("UserRole", loggedUser.RoleId);

            return RedirectToAction("Index", "Home");
        }

        // GET: Change Profile Page
        [HttpGet("ChangeProfile")]
        public IActionResult ChangeProfile()
        {
            var loggedUserId = HttpContext.Session.GetString("loggedUserId");

            if (loggedUserId == null)
            {
                TempData["Error"] = "You must be logged in.";
                return RedirectToAction("Login");
            }

            var dbUser = _usersRepo.FirstOrDefault(u => u.Id.ToString() == loggedUserId);

            if (dbUser == null)
            {
                TempData["Error"] = "User not found.";
                return RedirectToAction("Login");
            }

            var model = new ChangeProfileVM
            {
                Name = dbUser.Name
            };

            return View(model);
        }

        // POST: Change Profile
        [HttpPost("ChangeProfile")]
        public IActionResult ChangeProfile(ChangeProfileVM model)
        {
            var loggedUserId = HttpContext.Session.GetString("loggedUserId");

            if (loggedUserId == null)
            {
                TempData["Error"] = "You must be logged in.";
                return RedirectToAction("Login");
            }

            var dbUser = _usersRepo.FirstOrDefault(u => u.Id.ToString() == loggedUserId);

            if (dbUser == null)
            {
                TempData["Error"] = "User not found.";
                return RedirectToAction("Login");
            }

            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Please correct the errors in the form.";
                return View(model);
            }

            if (!string.IsNullOrWhiteSpace(model.Name))
            {
                dbUser.Name = model.Name;
                HttpContext.Session.SetString("UserName", model.Name);
            }

            if (!string.IsNullOrWhiteSpace(model.Password))
            {
                if (model.Password != model.ConfirmPassword)
                {
                    TempData["Error"] = "Passwords do not match.";
                    return View(model);
                }
                dbUser.Password = model.Password;
            }

            _usersRepo.Update(dbUser);

            TempData["Success"] = "Profile updated successfully.";
            return RedirectToAction("Index", "Admin");
        }

        // Logout User
        [HttpGet("Logout")]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
    }
}
