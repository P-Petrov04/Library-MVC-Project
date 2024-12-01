using Common.Entities;
using Common.Repositories;
using Microsoft.AspNetCore.Mvc;
using Web.ViewModels.Publishers;


namespace Web.Controllers
{
    public class PublisherController : Controller
    {
        private readonly BaseRepository<Publisher> _publisherRepo;
        private readonly BaseRepository<Book> _bookRepo;

        public PublisherController(BaseRepository<Publisher> publisherRepo, BaseRepository<Book> bookRepo)
        {
            _publisherRepo = publisherRepo;
            _bookRepo = bookRepo;
        }

        public IActionResult Index()
        {
            PublisherVM model = new PublisherVM()
            {
                Items = _publisherRepo.GetAll()
            };
            return View(model);
        }

        [HttpGet]
        public IActionResult AddPublisher()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AddPublisher(AddPublisherVM model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            Publisher publisher = new Publisher();

            // Check if publisher already exists
            if (_publisherRepo.FirstOrDefault(p => p.Name.Equals(model.Name)) != null)
            {
                TempData["Error"] = $"The publisher '{model.Name}' already exists.";
                return RedirectToAction("AddPublisher");
            }

            publisher.Name = model.Name;

            _publisherRepo.Add(publisher);
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> DeletePublisher(int id)
        {
            Publisher publisher = _publisherRepo.FirstOrDefault(p => p.Id == id);

            if (publisher != null)
            {
                foreach (var book in _bookRepo.GetAll().Where(b => b.PublisherId == publisher.Id))
                {
                    _bookRepo.Delete(book);
                }

                _publisherRepo.Delete(publisher);
            }

            return RedirectToAction("Index");
        }

        [HttpGet]
        public IActionResult EditPublisher(int id)
        {
            Publisher publisher = _publisherRepo.FirstOrDefault(p => p.Id == id);

            if (publisher == null)
            {
                return NotFound();
            }

            EditPublisherVM model = new EditPublisherVM()
            {
                Id = publisher.Id,
                Name = publisher.Name
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> EditPublisher(EditPublisherVM model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            Publisher publisher = _publisherRepo.FirstOrDefault(p => p.Id == model.Id);

            if (publisher == null)
            {
                return NotFound();
            }

            publisher.Name = model.Name;
            _publisherRepo.Update(publisher);

            return RedirectToAction("Index");
        }
    }
}
