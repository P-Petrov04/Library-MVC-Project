using Common.Entities;
using Common.Repositories;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
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

        // Helper method to check session and role
        private bool IsAuthorizedAdminOrModerator()
        {
            var userRole = HttpContext.Session.GetInt32("UserRole");

            
            return userRole != null && (userRole == 1 || userRole == 2); 
        }
        private bool IsAuthorizedAdmin()
        {
            var userRole = HttpContext.Session.GetInt32("UserRole");

            
            return userRole != null && userRole == 1; 
        }

        // GET: Admin Dashboard
        [HttpGet]
        public IActionResult Index()
        {
            if (!IsAuthorizedAdminOrModerator())
            {
                return RedirectToAction("Login", "Auth");
            }

            UserVM model = new UserVM
            {
                Items = _usersRepo.GetAll()
            };

            return View(model); 
        }

        // GET: Register User/Moderator Page
        [HttpGet]
        public IActionResult RegisterUser()
        {
            if (!IsAuthorizedAdmin())
            {
                return RedirectToAction("Login", "Auth");
            }

            return View();
        }

        // POST: Register User/Moderator
        [HttpPost]
        public async Task<IActionResult> RegisterUser(RegisterUserVM model)
        {
            if (!IsAuthorizedAdmin())
            {
                return RedirectToAction("Login", "Auth");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            User item = new User();

            
            if (!model.Role.Equals("Admin") && !model.Role.Equals("Moderator") &&
                !model.Role.Equals("Member"))
            {
                TempData["Error"] = $"Role '{model.Role}' does not exist.";
                return RedirectToAction("RegisterUser");
            }

            
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
            if (!IsAuthorizedAdmin())
            {
                return RedirectToAction("Login", "Auth");
            }

            User item = _usersRepo.FirstOrDefault(x => x.Id == id);

            if (item != null)
                _usersRepo.Delete(item);

            return RedirectToAction("UserList", "Auth");
        }
    }
}
