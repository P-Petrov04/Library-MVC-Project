﻿using Microsoft.AspNetCore.Mvc;
using Common.Repositories;
using Web.ViewModels.Auth;
using Common.Entities;

namespace Web.Controllers
{
    [Route("[controller]/[action]")]
    public class AuthController : Controller
    {
        private readonly BaseRepository<User> _usersRepo;

        public AuthController(BaseRepository<User> usersRepo)
        {
            _usersRepo = usersRepo;
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

            // Set session values
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

            // Update user information
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
