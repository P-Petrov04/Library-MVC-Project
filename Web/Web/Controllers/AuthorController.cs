using Common.Entities;
using Common.Repositories;
using Microsoft.AspNetCore.Mvc;
using Web.ViewModels.Authors;

namespace Web.Controllers
{
    public class AuthorController : Controller
    {
        private readonly BaseRepository<Author> _authorRepo;

        public AuthorController(BaseRepository<Author> authorRepo)
        {
            _authorRepo = authorRepo;
        }

        // Helper method to check session and role
        private bool IsAuthorized()
        {
            var userRole = HttpContext.Session.GetInt32("UserRole");
            return userRole == 1 || userRole == 2; // Admin (1) or Moderator (2)
        }

        private IActionResult UnauthorizedRedirect()
        {
            TempData["Error"] = "You are not authorized to access this page.";
            return RedirectToAction("Login", "Auth");
        }

        // GET: Author List
        public IActionResult Index()
        {
            if (!IsAuthorized())
            {
                return UnauthorizedRedirect();
            }

            AuthorVM model = new AuthorVM()
            {
                Items = _authorRepo.GetAll()
            };
            return View(model);
        }

        // GET: Add Author Page
        [HttpGet]
        public IActionResult AddAuthor()
        {
            if (!IsAuthorized())
            {
                return UnauthorizedRedirect();
            }

            return View();
        }

        // POST: Add Author
        [HttpPost]
        public async Task<IActionResult> AddAuthor(AddAuthorVM model)
        {
            if (!IsAuthorized())
            {
                return UnauthorizedRedirect();
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Check if author exists
            if (_authorRepo.FirstOrDefault(a => a.Name.Equals(model.Name)) != null)
            {
                TempData["Error"] = $"The author '{model.Name}' already exists.";
                return RedirectToAction("AddAuthor");
            }

            // Add new author
            Author author = new Author
            {
                Name = model.Name,
                Bio = model.Bio
            };

            _authorRepo.Add(author);
            TempData["Success"] = $"Author '{author.Name}' added successfully!";
            return RedirectToAction("Index");
        }

        // POST: Delete Author
        [HttpPost]
        public async Task<IActionResult> DeleteAuthor(int id)
        {
            if (!IsAuthorized())
            {
                return UnauthorizedRedirect();
            }

            Author author = _authorRepo.FirstOrDefault(a => a.Id == id);

            if (author != null)
            {
                _authorRepo.Delete(author);
            }
            TempData["Success"] = $"Author '{author.Name}' deleted successfully!";

            return RedirectToAction("Index");
        }

        // GET: Edit Author Page
        [HttpGet]
        public IActionResult EditAuthor(int id)
        {
            if (!IsAuthorized())
            {
                return UnauthorizedRedirect();
            }

            Author author = _authorRepo.FirstOrDefault(a => a.Id == id);

            if (author == null)
            {
                return NotFound();
            }

            EditAuthorVM model = new EditAuthorVM
            {
                Id = author.Id,
                Name = author.Name,
                Bio = author.Bio
            };

            return View(model);
        }

        // POST: Edit Author
        [HttpPost]
        public async Task<IActionResult> EditAuthor(EditAuthorVM model)
        {
            if (!IsAuthorized())
            {
                return UnauthorizedRedirect();
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            Author author = _authorRepo.FirstOrDefault(a => a.Id == model.Id);

            if (author == null)
            {
                return NotFound();
            }

            // Update author details
            author.Name = model.Name;
            author.Bio = model.Bio;
            _authorRepo.Update(author);

            TempData["Success"] = $"Author '{author.Name}' edited successfully!";
            return RedirectToAction("Index");
        }
    }
}
