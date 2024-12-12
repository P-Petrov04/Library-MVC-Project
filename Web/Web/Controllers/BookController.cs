using Common.Entities;
using Common.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Web.ViewModels.Books;

namespace Web.Controllers
{
   // [Authorize(Roles = "Admin")] 
    public class BookController : Controller
    {
        private readonly BaseRepository<Book> _bookRepo;
        private readonly BaseRepository<Author> _authorRepo;
        private readonly BaseRepository<BookAuthor> _bookAuthorRepo;
        private readonly BaseRepository<BookCategory> _bookCategoryRepo; 
        private readonly BaseRepository<Category> _categoryRepo;
        private readonly BaseRepository<Publisher> _publisherRepo;

        public BookController(
            BaseRepository<Book> bookRepo,
            BaseRepository<Author> authorRepo,
            BaseRepository<BookAuthor> bookAuthorRepo,
            BaseRepository<BookCategory> bookCategoryRepo,
            BaseRepository<Category> categoryRepo,
            BaseRepository<Publisher> publisherRepo)
        {
            _bookRepo = bookRepo;
            _authorRepo = authorRepo;
            _bookAuthorRepo = bookAuthorRepo;
            _bookCategoryRepo = bookCategoryRepo;
            _categoryRepo = categoryRepo;
            _publisherRepo = publisherRepo;
        }

        [HttpGet]
        public IActionResult Index()
        {
            var books = _bookRepo.GetAll();
            return View(books); 
        }

        [HttpGet]
        public IActionResult AddBook()
        {
         
            ViewBag.Authors = _authorRepo.GetAll();
            ViewBag.Categories = _categoryRepo.GetAll();
            ViewBag.Publishers = _publisherRepo.GetAll();

            return View();
        }

        [HttpPost]
        public IActionResult AddBook(AddBookVM model)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Invalid input.";
                return RedirectToAction("AddBook");
            }

         
            string? fileName = null;
            if (model.CoverImage != null)
            {
                fileName = Guid.NewGuid().ToString() + Path.GetExtension(model.CoverImage.FileName);
                string filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images", fileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    model.CoverImage.CopyTo(stream);
                }
            }

         
            var book = new Book
            {
                Title = model.Title,
                ISBN = model.ISBN,
                Summary = model.Summary,
                PublisherId = model.PublisherId,
                CoverImagePath = fileName,
                PublishedDate = model.PublishedDate
            };

            _bookRepo.Add(book);

            
            foreach (var authorId in model.AuthorIds)
            {
                _bookAuthorRepo.Add(new BookAuthor { BookId = book.Id, AuthorId = authorId });
            }

            // Add many-to-many relationships (Categories)
            foreach (var categoryId in model.CategoryIds)
            {
                _bookCategoryRepo.Add(new BookCategory { BookId = book.Id, CategoryId = categoryId });
            }

            TempData["Success"] = "Book added successfully!";
            return RedirectToAction("Index");
        }
    }
}
