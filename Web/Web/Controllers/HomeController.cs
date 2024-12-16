using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using Common.Entities;
using Common.Repositories;

namespace Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly BaseRepository<Book> _bookRepo;
        private readonly BaseRepository<Author> _authorRepo;
        private readonly BaseRepository<BookAuthor> _bookAuthorRepo;
        private readonly BaseRepository<Publisher> _publisherRepo;
        private readonly BaseRepository<Tag> _tagRepo;
        private readonly BaseRepository<Category> _categoryRepo;
        private readonly BaseRepository<BookCategory> _bookCategoryRepo;

        public HomeController(BaseRepository<Book> bookRepo,
                              BaseRepository<Author> authorRepo,
                              BaseRepository<BookAuthor> bookAuthorRepo,
                              BaseRepository<Publisher> publisherRepo,
                              BaseRepository<Tag> tagRepo,
                              BaseRepository<Category> categoryRepo,
                              BaseRepository<BookCategory> bookCategoryRepo)
        {
            _bookRepo = bookRepo;
            _authorRepo = authorRepo;
            _bookAuthorRepo = bookAuthorRepo;
            _publisherRepo = publisherRepo;
            _tagRepo = tagRepo;
            _categoryRepo = categoryRepo;
            _bookCategoryRepo = bookCategoryRepo;
        }

        public IActionResult Index() 
        { 
            var books = _bookRepo.GetAll()
                .Select(book => new { book.Id, book.Title, book.CoverImagePath, book.PublisherId, Authors = _bookAuthorRepo.GetAll()
                .Where(ba => ba.BookId == book.Id)
                .Join(_authorRepo.GetAll(), ba => ba.AuthorId, a => a.Id, (ba, a) => a.Name).ToList(), 
                 Publisher = _publisherRepo.FirstOrDefault(p => p.Id == book.PublisherId)?.Name, Tags = _tagRepo.GetAll()
                 .Where(t => t.CategoryId == book.Id).Select(t => t.Name).ToList(), Categories = _bookCategoryRepo.GetAll()
                 .Where(bc => bc.BookId == book.Id).Join(_categoryRepo.GetAll(), bc => bc.CategoryId, c => c.Id, (bc, c) => c.Name).ToList() 
                }).ToList(); 
            
            return View(books); 
        }


        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public IActionResult Details(int id) 
        { 
            var book = _bookRepo.GetAll()
                .Where(b => b.Id == id)
                .Select(book => new { book.Id, book.Title, book.CoverImagePath, book.PublisherId, Authors = _bookAuthorRepo.GetAll()
                .Where(ba => ba.BookId == book.Id).Join(_authorRepo.GetAll(), ba => ba.AuthorId, a => a.Id, (ba, a) => a.Name)
                .ToList(), Publisher = _publisherRepo.FirstOrDefault(p => p.Id == book.PublisherId)?.Name, Tags = _tagRepo.GetAll()
                .Where(t => t.CategoryId == book.Id).Select(t => t.Name).ToList(), Categories = _bookCategoryRepo.GetAll()
                .Where(bc => bc.BookId == book.Id).Join(_categoryRepo.GetAll(), bc => bc.CategoryId, c => c.Id, (bc, c) => c.Name)
                .ToList() }).FirstOrDefault(); 
            
            if (book == null) 
            { 
                return NotFound(); 
            } 
            
            return View(book); }

    }
}
