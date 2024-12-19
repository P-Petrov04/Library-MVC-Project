using Common.Entities;
using Common.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Web.ViewModels.Users;

namespace Web.Controllers
{
    public class AdminController : Controller
    {
        private readonly BaseRepository<User> _usersRepo;
        private readonly BaseRepository<Role> _rolesRepo;

        public AdminController(BaseRepository<User> usersRepo, BaseRepository<Role> rolesRepo)
        {
            _usersRepo = usersRepo;
            _rolesRepo = rolesRepo;
        }

        // GET: Admin Dashboard
        [HttpGet]
        public IActionResult Index()
        {
            UserVM model = new UserVM
            {
                Items = _usersRepo.GetAll()
            };

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
            if (!ModelState.IsValid)
            {
                return View(model);
            }

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
            var currRole = _rolesRepo.FirstOrDefault(r => r.Name == model.Role);
            if (currRole == null)
            {
                TempData["Error"] = $"Role '{model.Role}' does not exist.";
                return RedirectToAction("RegisterUser");
            }
            item.RoleId = currRole.Id;

            _usersRepo.Add(item);

            return RedirectToAction("Index");
        }

        // POST: Delete User
        [HttpPost]
        public async Task<IActionResult> DeleteUser(int id)
        {
            User item = _usersRepo.FirstOrDefault(x => x.Id == id);

            if (item != null)
                _usersRepo.Delete(item);

            return RedirectToAction("Index");
        }
    }
}
