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
        public IActionResult Index()
        {
            if (!IsAuthorized())
            {
                return UnauthorizedRedirect();
            }

            PublisherVM model = new PublisherVM()
            {
                Items = _publisherRepo.GetAll()
            };
            return View(model);
        }

        [HttpGet]
        public IActionResult AddPublisher()
        {
            if (!IsAuthorized())
            {
                return UnauthorizedRedirect();
            }

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AddPublisher(AddPublisherVM model)
        {
            if (!IsAuthorized())
            {
                return UnauthorizedRedirect();
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            Publisher publisher = new Publisher();

            
            if (_publisherRepo.FirstOrDefault(p => p.Name.Equals(model.Name)) != null)
            {
                TempData["Error"] = $"The publisher '{model.Name}' already exists.";
                return RedirectToAction("AddPublisher");
            }

            publisher.Name = model.Name;
            publisher.Address = model.Address;

            _publisherRepo.Add(publisher);
            TempData["Success"] = $"Publisher '{publisher.Name}' added successfully!";
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> DeletePublisher(int id)
        {
            if (!IsAuthorized())
            {
                return UnauthorizedRedirect();
            }

            Publisher publisher = _publisherRepo.FirstOrDefault(p => p.Id == id);

            if (publisher != null)
            {
                foreach (var book in _bookRepo.GetAll().Where(b => b.PublisherId == publisher.Id))
                {
                    _bookRepo.Delete(book);
                }
                TempData["Success"] = $"Publisher '{publisher.Name}' deleted successfully!";
                _publisherRepo.Delete(publisher);
            }

            return RedirectToAction("Index");
        }

        [HttpGet]
        public IActionResult EditPublisher(int id)
        {
            if (!IsAuthorized())
            {
                return UnauthorizedRedirect();
            }

            Publisher publisher = _publisherRepo.FirstOrDefault(p => p.Id == id);

            if (publisher == null)
            {
                return NotFound();
            }

            EditPublisherVM model = new EditPublisherVM()
            {
                Id = publisher.Id,
                Name = publisher.Name,
                Address = publisher.Address
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> EditPublisher(EditPublisherVM model)
        {
            if (!IsAuthorized())
            {
                return UnauthorizedRedirect();
            }

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
            publisher.Address = model.Address;
            _publisherRepo.Update(publisher);

            TempData["Success"] = $"Publisher '{publisher.Name}' edited successfully!";
            return RedirectToAction("Index");
        }
    }
}
