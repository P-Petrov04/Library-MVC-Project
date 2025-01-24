using Common.Entities;
using Common.Repositories;
using Microsoft.AspNetCore.Mvc;
using Web.ViewModels.Tags;

namespace Web.Controllers
{
    public class TagController : Controller
    {
        private readonly BaseRepository<Tag> _tagRepo;
        private readonly BaseRepository<Category> _categoryRepo;

        public TagController(BaseRepository<Tag> tagRepo, BaseRepository<Category> categoryRepo)
        {
            _tagRepo = tagRepo;
            _categoryRepo = categoryRepo;
        }

        private bool IsAuthorized()
        {
            var userRole = HttpContext.Session.GetInt32("UserRole");
            return userRole == 1 || userRole == 2; 
        }

        private IActionResult UnauthorizedRedirect()
        {
            TempData["Error"] = "You are not authorized to access this page.";
            return RedirectToAction("Login", "Auth");
        }

        [HttpGet]
        public IActionResult Index()
        {
            if (!IsAuthorized())
            {
                return UnauthorizedRedirect();
            }

            TagVM tags = new TagVM()
            {
                Items = _tagRepo.GetAll()
            };
            return View(tags);
        }

        [HttpGet]
        public IActionResult AddTag()
        {
            if (!IsAuthorized())
            {
                return UnauthorizedRedirect();
            }

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AddTag(AddTagVM model)
        {
            if (!IsAuthorized())
            {
                return UnauthorizedRedirect();
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            Tag tag = new Tag();

            if (_tagRepo.FirstOrDefault(t => t.Name.ToLower() == model.Name.ToLower()) != null)
            {
                TempData["Error"] = $"This '{model.Name}' already exists.";
                return RedirectToAction("AddTag");
            }

            var category = _categoryRepo.FirstOrDefault(c => c.Name.ToLower() == model.Category.ToLower());
            if (category == null)
            {
                TempData["Error"] = $"This '{model.Category}' does not exist.";
                return RedirectToAction("AddTag");
            }

            tag.Name = model.Name;
            tag.CategoryId = category.Id;

            _tagRepo.Add(tag);
            TempData["Success"] = $"Tag '{tag.Name}' added successfully!";
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> DeleteTag(int id)
        {
            if (!IsAuthorized())
            {
                return UnauthorizedRedirect();
            }

            Tag item = _tagRepo.FirstOrDefault(x => x.Id == id);

            if (item != null)
            {
                TempData["Success"] = $"Tag '{item.Name}' deleted successfully!";
                _tagRepo.Delete(item);
            }
                


            return RedirectToAction("Index");
        }

        [HttpGet]
        public IActionResult EditTag(int id)
        {
            if (!IsAuthorized())
            {
                return UnauthorizedRedirect();
            }

            Tag tag = _tagRepo.FirstOrDefault(t => t.Id == id);

            if (tag == null)
            {
                TempData["Error"] = "Tag not found.";
                return RedirectToAction("Index");
            }

            EditTagVM model = new EditTagVM
            {
                Id = id,
                Name = tag.Name,
                CategoryId = tag.CategoryId,
                CategoryName = _categoryRepo.FirstOrDefault(c => c.Id == tag.CategoryId)?.Name
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> EditTag(EditTagVM model)
        {
            if (!IsAuthorized())
            {
                return UnauthorizedRedirect();
            }

            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Please correct the errors.";
                return RedirectToAction("EditTag", new { id = model.Id });
            }

            Tag tag = _tagRepo.FirstOrDefault(t => t.Id == model.Id);

            if (tag == null)
            {
                TempData["Error"] = "Tag not found.";
                return RedirectToAction("Index");
            }

            tag.Name = model.Name;

         
            var category = _categoryRepo.FirstOrDefault(c => c.Id == model.CategoryId);
            if (category == null)
            {
                TempData["Error"] = "Selected category does not exist.";
                return RedirectToAction("EditTag", new { id = model.Id });
            }

            tag.CategoryId = model.CategoryId;

            _tagRepo.Update(tag);

            TempData["Success"] = $"Tag '{tag.Name}' edited successfully.";
            return RedirectToAction("Index");
        }
    }
}
