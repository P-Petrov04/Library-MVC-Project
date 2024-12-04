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

        
        public IActionResult Index()
        {
            AuthorVM model = new AuthorVM()
            {
                Items = _authorRepo.GetAll()
            };
            return View(model);
        }

        
        [HttpGet]
        public IActionResult AddAuthor()
        {
            return View();
        }

        
        [HttpPost]
        public async Task<IActionResult> AddAuthor(AddAuthorVM model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            
            if (_authorRepo.FirstOrDefault(a => a.Name.Equals(model.Name)) != null)
            {
                TempData["Error"] = $"The author '{model.Name}' already exists.";
                return RedirectToAction("AddAuthor");
            }

            
            Author author = new Author
            {
                Name = model.Name,
                Bio = model.Bio
            };

            _authorRepo.Add(author);
            return RedirectToAction("Index");
        }

        
        [HttpPost]
        public async Task<IActionResult> DeleteAuthor(int id)
        {
            Author author = _authorRepo.FirstOrDefault(a => a.Id == id);

            if (author != null)
            {
                _authorRepo.Delete(author);
            }

            return RedirectToAction("Index");
        }

        
        [HttpGet]
        public IActionResult EditAuthor(int id)
        {
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

        
        [HttpPost]
        public async Task<IActionResult> EditAuthor(EditAuthorVM model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            Author author = _authorRepo.FirstOrDefault(a => a.Id == model.Id);

            if (author == null)
            {
                return NotFound();
            }

            author.Name = model.Name;
            author.Bio = model.Bio;
            _authorRepo.Update(author);

            return RedirectToAction("Index");
        }
    }
}
