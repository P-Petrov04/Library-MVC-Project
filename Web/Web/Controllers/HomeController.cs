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

        public HomeController(BaseRepository<Book> bookRepo,
                              BaseRepository<Author> authorRepo,
                              BaseRepository<BookAuthor> bookAuthorRepo)
        {
            _bookRepo = bookRepo;
            _authorRepo = authorRepo;
            _bookAuthorRepo = bookAuthorRepo;
        }

        public IActionResult Index()
        {
            var books = _bookRepo.GetAll().Select(book => new
            {
                book.Title,
                book.CoverImagePath,
                Authors = _bookAuthorRepo.GetAll()
                    .Where(ba => ba.BookId == book.Id)
                    .Join(_authorRepo.GetAll(),
                          ba => ba.AuthorId,
                          a => a.Id,
                          (ba, a) => a.Name)
                    .ToList()
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
    }
}
