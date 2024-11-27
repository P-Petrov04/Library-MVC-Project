using Common.Entities;
using Common.Repositories;
using Microsoft.AspNetCore.Mvc;
using Web.ViewModels.Categories;

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
        public IActionResult Index()
        {
            CategoryVM model = new CategoryVM()
            {
                Items = _categoryRepo.GetAll()
            };
            return View(model);
        }

        [HttpGet]
        public IActionResult AddCategory()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AddCategory(AddCategoryVM model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            Category category = new Category();

            //check is category already exist
            if (_categoryRepo.FirstOrDefault(c => c.Name.Equals(model.Name)) != null)
            {
                TempData["Error"] = $"This '{model.Name}' already exist.";
                return RedirectToAction("AddCategory");
            }

            category.Name = model.Name;

            _categoryRepo.Add(category);
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            Category item = _categoryRepo.FirstOrDefault(x => x.Id == id);

            if (item != null)
            {
                foreach (var tag in _tagRepo.GetAll().Where(t => t.CategoryId == item.Id))
                {
                    _tagRepo.Delete(tag);
                }

                _categoryRepo.Delete(item);
            }

            return RedirectToAction("Index");
        }

        [HttpGet]
        public IActionResult EditCategory(int id)
        {
            Category category = _categoryRepo.FirstOrDefault(c => c.Id == id);

            EditCategoryVM model = new EditCategoryVM();
            model.Id = category.Id;
            model.Name = category.Name;

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> EditCategory(EditCategoryVM model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            Category category = _categoryRepo.FirstOrDefault(c => c.Id == model.Id);

            category.Name = model.Name;
            _categoryRepo.Update(category);

            return RedirectToAction("Index");
        }
    }
}
