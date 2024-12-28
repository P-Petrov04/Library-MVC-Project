using Common.Entities;
using Common.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Web.ViewModels.Authors;
using Web.ViewModels.Categories;
using Web.ViewModels.Publishers;

namespace Web.Controllers
{
    public class CategoryController : Controller
    {
        private readonly BaseRepository<Category> _categoryRepo;
        private readonly BaseRepository<Tag> _tagRepo;

        public CategoryController(BaseRepository<Category> categoryRepo, BaseRepository<Tag> tagRepo)
        {
            _categoryRepo = categoryRepo;
            _tagRepo = tagRepo;
        }

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

        public IActionResult Index()
        {
            if (!IsAuthorized())
            {
                return UnauthorizedRedirect();
            }

            CategoryVM model = new CategoryVM()
            {
                Items = _categoryRepo.GetAll()
            };
            return View(model);
        }

        [HttpGet]
        public IActionResult AddCategory()
        {
            if (!IsAuthorized())
            {
                return UnauthorizedRedirect();
            }

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            if (!IsAuthorized())
            {
                return UnauthorizedRedirect();
            }

            Category item = _categoryRepo.FirstOrDefault(x => x.Id == id);


            if (item != null)
            {
                // Delete all associated tags
                foreach (var tag in _tagRepo.GetAll().Where(t => t.CategoryId == item.Id))
                {
                    _tagRepo.Delete(tag);

                }

                _categoryRepo.Delete(item);
                TempData["Success"] = $"Category '{item.Name}' deleted successfully!";
            }

            return RedirectToAction("Index");
        }

        [HttpGet]
        public IActionResult EditCategory(int id)
        {
            if (!IsAuthorized())
            {
                return UnauthorizedRedirect();
            }

            Category category = _categoryRepo.FirstOrDefault(c => c.Id == id);

            if (category == null)
                return NotFound();

            EditCategoryVM model = new EditCategoryVM
            {
                Id = category.Id,
                Name = category.Name
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> EditCategory(EditCategoryVM model)
        {
            if (!IsAuthorized())
            {
                return UnauthorizedRedirect();
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            Category category = _categoryRepo.FirstOrDefault(c => c.Id == model.Id);

            if (category == null)
                return NotFound();

            category.Name = model.Name;
            _categoryRepo.Update(category);

            TempData["Success"] = $"Category '{category.Name}' edited successfully!";
            return RedirectToAction("Index");
        }
    }
}
