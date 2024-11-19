using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Web.Data;

namespace Web.Controllers
{
    public class AuthController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly ApplicationDbContext _context;

        public AuthController(UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
        }

        // GET: Login Page
        [HttpGet]
        public IActionResult Login()
        {
            // Redirect if user is already logged in
            if (HttpContext.Session.GetString("UserEmail") != null)
            {
                return RedirectToAction("Index", "Home");
            }

            return View();
        }

        // POST: Login User
        [HttpPost]
        public async Task<IActionResult> Login(string email, string password)
        {
            var user = await _userManager.FindByEmailAsync(email);

            if (user == null)
            {
                TempData["Error"] = "User does not exist.";
                return View();
            }

            if (await _userManager.CheckPasswordAsync(user, password))
            {
                var result = await _signInManager.PasswordSignInAsync(user, password, isPersistent: true, lockoutOnFailure: false);

                if (result.Succeeded)
                {
                    var roles = await _userManager.GetRolesAsync(user);

                    // Retrieve user's name from the `User` table in the database
                    var dbUser = _context.User.FirstOrDefault(u => u.Email == email);

                    if (dbUser != null)
                    {
                        HttpContext.Session.SetString("UserName", dbUser.Name); // Store name in session
                        HttpContext.Session.SetString("UserEmail", email);
                        HttpContext.Session.SetString("UserRole", roles.FirstOrDefault() ?? "User");
                    }

                    if (roles.Contains("Admin"))
                    {
                        return RedirectToAction("Index", "Admin");
                    }
                    else if (roles.Contains("Moderator"))
                    {
                        return RedirectToAction("Index", "Moderator");
                    }
                    else
                    {
                        return RedirectToAction("Index", "Home");
                    }
                }
            }

            TempData["Error"] = "Invalid login attempt.";
            return View();
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

            var dbUser = _context.User.FirstOrDefault(u => u.Email == email);
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
        public async Task<IActionResult> ChangeProfile(string name, string password, string confirmPassword)
        {
            var email = HttpContext.Session.GetString("UserEmail");

            if (email == null)
            {
                TempData["Error"] = "You must be logged in.";
                return RedirectToAction("Login");
            }

            var dbUser = _context.User.FirstOrDefault(u => u.Email == email);
            if (dbUser == null)
            {
                TempData["Error"] = "User not found.";
                return RedirectToAction("Login");
            }

            // Update name
            if (!string.IsNullOrWhiteSpace(name))
            {
                dbUser.Name = name;
                HttpContext.Session.SetString("UserName", name);
            }

            // Update password
            if (!string.IsNullOrWhiteSpace(password))
            {
                if (password != confirmPassword)
                {
                    TempData["Error"] = "Passwords do not match.";
                    return View(dbUser);
                }

                var identityUser = await _userManager.FindByEmailAsync(email);
                if (identityUser != null)
                {
                    var resetToken = await _userManager.GeneratePasswordResetTokenAsync(identityUser);
                    var passwordResult = await _userManager.ResetPasswordAsync(identityUser, resetToken, password);

                    if (!passwordResult.Succeeded)
                    {
                        TempData["Error"] = string.Join(", ", passwordResult.Errors.Select(e => e.Description));
                        return View(dbUser);
                    }
                }
            }

            _context.User.Update(dbUser);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Profile updated successfully.";
            return RedirectToAction("ChangeProfile");
        }

        // Logout User
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            HttpContext.Session.Clear(); // Clear all session data
            return RedirectToAction("Login", "Auth");
        }
    }
}
