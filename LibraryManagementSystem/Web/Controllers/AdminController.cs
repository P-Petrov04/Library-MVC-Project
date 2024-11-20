using Common.Entities;
using Common.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Web.ViewModels.Users;

namespace Web.Controllers
{
    //[Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        //private readonly ApplicationDbContext _context;

        // GET: Admin Dashboard
        [HttpGet]
        public IActionResult Index()
        {
            BaseRepository<User> usersRepo = new BaseRepository<User>();
            UserVM model = new UserVM();
            model.Items = usersRepo.GetAll();

            return View(model); // Pass list of users to the view
        }

        // GET: Register User/Moderator Page
        [HttpGet]
        public IActionResult RegisterUser()
        {
            return View();
        }

        // POST: Register User/Moderator
        [HttpPost]
        public async Task<IActionResult> RegisterUser(RegisterUserVM model)
        {
            if (ModelState.IsValid) 
            { 
                return View(model);
            }

            BaseRepository<User> usersRepo = new BaseRepository<User>();
            BaseRepository<Role> rolesRepo = new BaseRepository<Role>();
            User item = new User();

            // Ensure the role exists
            if (!model.Role.Equals("Admin") && !model.Role.Equals("Moderator") &&
                !model.Role.Equals("Member"))
            {
                TempData["Error"] = $"Role '{model.Role}' does not exist.";
                return RedirectToAction("RegisterUser");
            }

            // Create the user
            item.Email = model.Email;
            item.Name = model.Name;
            item.Password = model.Password;
            Role currRole = rolesRepo.FirstOrDefault(r => r.Name == model.Role);
            item.Role = currRole;

            usersRepo.Add(item);

            return RedirectToAction("Index");
        }

        // POST: Delete User
        [HttpPost]
        public async Task<IActionResult> DeleteUser(int id)
        {
            BaseRepository<User> usersRepo = new BaseRepository<User>();

            User item = usersRepo.FirstOrDefault(x => x.Id == id);

            if(item != null)
                usersRepo.Delete(item);

            return RedirectToAction("Action");
        }
    }
}
