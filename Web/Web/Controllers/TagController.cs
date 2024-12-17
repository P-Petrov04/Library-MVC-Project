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

        [HttpGet]
        public IActionResult Index()
        {
            TagVM tags = new TagVM()
            {
                Items = _tagRepo.GetAll()
            };
            return View(tags);
        }

        [HttpGet]
        public IActionResult AddTag()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AddTag(AddTagVM model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            Tag tag = new Tag();

            // Check if a tag with the same name already exists
            if (_tagRepo.FirstOrDefault(t => t.Name.ToLower() == model.Name.ToLower()) != null)
            {
                TempData["Error"] = $"This '{model.Name}' already exists.";
                return RedirectToAction("AddTag");
            }

            // Check if the category exists
            var category = _categoryRepo.FirstOrDefault(c => c.Name.ToLower() == model.Category.ToLower());
            if (category == null)
            {
                TempData["Error"] = $"This '{model.Category}' does not exist.";
                return RedirectToAction("AddTag");
            }

            tag.Name = model.Name;
            tag.CategoryId = category.Id;

            _tagRepo.Add(tag);
            TempData["Success"] = "Tag added successfully!";
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> DeleteTag(int id)
        {
            Tag item = _tagRepo.FirstOrDefault(x => x.Id == id);

            if (item != null)
                _tagRepo.Delete(item);

            return RedirectToAction("Index");
        }

        [HttpGet]
        public IActionResult EditTag(int id)
        {
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
            TempData["Success"] = "Tag updated successfully.";
            return RedirectToAction("Index");
        }
    }
}
