using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Common.Repositories;
using Web.ViewModels.Auth;
using Common.Entities;
using System;

namespace Web.Controllers
{
    public class AuthController : Controller
    {

        // GET: Login Page
        [HttpGet]
        public IActionResult Login(string url)
        {
            // Redirect if user is already logged in
            if (HttpContext.Session.GetString("UserEmail") != null)
            {
                return RedirectToAction("Index", "Home");
            }
            ViewData["url"] = url;
            return View();
        }

        // POST: Login User
        [HttpPost]
        public IActionResult Login(LoginVM model)
        {
            // Check if the user is already logged in (i.e., session contains loggedUserId)
            if (HttpContext.Session.GetString("loggedUserId") != null)
            {
                // If already logged in, redirect to Admin index page
                return RedirectToAction("Index", "Admin");
            }

            // Check if model state is valid
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Use your repository to get the user by email and password
            BaseRepository<User> usersRepo = new BaseRepository<User>();
            User loggedUser = usersRepo.FirstOrDefault(u =>
                        u.Email == model.Email && u.Password == model.Password);

            // If user not found, add error to ModelState and return view
            if (loggedUser == null)
            {
                ModelState.AddModelError("authError", "Invalid login attempt.");
                return View(model);
            }

            // Store user ID in session after successful login
            this.HttpContext.Session.SetString("loggedUserId", loggedUser.Id.ToString());

            // Redirect to Admin index page
            return RedirectToAction("Index", "Admin");
        }


        // GET: Change Profile Page
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public IActionResult ChangeProfile()
        {
            var email = HttpContext.Session.GetString("UserEmail");

            if (email == null)
            {
                TempData["Error"] = "You must be logged in.";
                return RedirectToAction("Login");
            }

            BaseRepository<User> usersRepo = new BaseRepository<User>();

            var dbUser = usersRepo.FirstOrDefault(u => u.Email == email);
            if (dbUser == null)
            {
                TempData["Error"] = "User not found.";
                return RedirectToAction("Login");
            }

            return View(dbUser);
        }

        // POST: Change Profile
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> ChangeProfile(ChangeProfileVM model)
        {
            var email = HttpContext.Session.GetString("UserEmail");

            if (email == null)
            {
                TempData["Error"] = "You must be logged in.";
                return RedirectToAction("Login");
            }

            BaseRepository<User> usersRepo = new BaseRepository<User>();

            var dbUser = usersRepo.FirstOrDefault(u => u.Email == email);
            if (dbUser == null)
            {
                TempData["Error"] = "User not found.";
                return RedirectToAction("Login");
            }

            // Update name
            if (!string.IsNullOrWhiteSpace(model.Name))
            {
                dbUser.Name = model.Name;
                HttpContext.Session.SetString("UserName", model.Name);
            }

            // Update password
            if (!string.IsNullOrWhiteSpace(model.Password))
            {
                if (model.Password != dbUser.Password)
                {
                    TempData["Error"] = "Passwords do not match.";
                    return View(dbUser);
                }

                dbUser.Password = model.Password;
            }

            usersRepo.Update(dbUser);

            TempData["Success"] = "Profile updated successfully.";
            return RedirectToAction("ChangeProfile");
        }

        // Logout User
        public async Task<IActionResult> Logout()
        {
            this.HttpContext.Session.Remove("loggedUserId");
            return RedirectToAction("Login", "Auth");
        }
    }
}
