﻿using Microsoft.AspNetCore.Mvc;
using Common.Repositories;
using Web.ViewModels.Auth;
using Common.Entities;
using Web.ViewModels.Users;
using Microsoft.AspNetCore.Http;

namespace Web.Controllers
{
    [Route("[controller]/[action]")]
    public class AuthController : Controller
    {
        private readonly BaseRepository<User> _usersRepo;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AuthController(BaseRepository<User> usersRepo, IHttpContextAccessor httpContextAccessor)
        {
            _usersRepo = usersRepo;
            _httpContextAccessor = httpContextAccessor;
        }

        // GET: User List
        [HttpGet]
        public IActionResult UserList()
        {
            if (!IsAdmin())
            {
                TempData["Error"] = "You must be an admin to access this page.";
                return RedirectToAction("Login");
            }

            var userVM = new UserVM
            {
                Items = _usersRepo.GetAll().ToList()
            };

            return View(userVM);
        }

        private bool IsAdmin()
        {
            return HttpContext.Session.GetInt32("UserRole") == 1; 
        }

        // GET: Edit User
        [HttpGet]
        public IActionResult EditUser(int id)
        {
            if (!IsAdmin())
            {
                TempData["Error"] = "You must be an admin to access this page.";
                return RedirectToAction("Login");
            }

            var user = _usersRepo.FirstOrDefault(u => u.Id == id);
            if (user == null)
            {
                TempData["Error"] = "User not found.";
                return RedirectToAction("UserList");
            }

            var model = new EditUserVM
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
                RoleId = user.RoleId
            };

            return View(model);
        }

        // POST: Edit User
        [HttpPost]
        public async Task<IActionResult> EditUser(EditUserVM model)
        {
            if (!IsAdmin())
            {
                TempData["Error"] = "You must be an admin to access this page.";
                return RedirectToAction("Login");
            }

            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Please correct the errors.";
                return RedirectToAction("EditUser", new { id = model.Id });
            }

            var user = _usersRepo.FirstOrDefault(u => u.Id == model.Id);
            if (user == null)
            {
                TempData["Error"] = "User not found.";
                return RedirectToAction("UserList");
            }

            user.Name = model.Name;
            user.Email = model.Email;
            user.RoleId = model.RoleId;

            _usersRepo.Update(user);

            TempData["Success"] = "User updated successfully.";
            return RedirectToAction("UserList");
        }

        // GET: Login Page
        [HttpGet]
        public IActionResult Login(string url = "/Home/Index")
        {
            if (HttpContext.Session.GetString("loggedUserId") != null)
            {
                return Redirect(url);
            }

            ViewData["ReturnUrl"] = url;
            return View();
        }

        // POST: Login User
        [HttpPost]
        public IActionResult Login(LoginVM model)
        {
            if (HttpContext.Session.GetString("loggedUserId") != null)
            {
                return RedirectToAction("Index", "Home");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var loggedUser = _usersRepo.FirstOrDefault(u =>
                u.Email == model.Email && u.Password == model.Password);

            if (loggedUser == null)
            {
                ModelState.AddModelError("authError", "Invalid email or password.");
                return View(model);
            }

            
            HttpContext.Session.SetString("loggedUserId", loggedUser.Id.ToString());
            HttpContext.Session.SetString("UserEmail", loggedUser.Email);
            HttpContext.Session.SetString("UserName", loggedUser.Name);
            HttpContext.Session.SetInt32("UserRole", loggedUser.RoleId);

            return RedirectToAction("Index", "Home");
        }

        // GET: Change Profile Page
        [HttpGet]
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
        [HttpPost]
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
            return RedirectToAction("ChangeProfile");
        }

        // GET: Logout User
        [HttpGet]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
    }
}
