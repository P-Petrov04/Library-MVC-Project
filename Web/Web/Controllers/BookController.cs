using Common.Entities;
using Common.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Web.ViewModels.Books;

namespace Web.Controllers
{
    [Authorize(Roles = "Admin")] // Restrict access to Admins (optional)
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
            var books = _bookRepo.GetAll(); // Fetch all books
            return View(books); // Pass books to the Index view
        }

        [HttpGet]
        public IActionResult AddBook()
        {
            // Prepare dropdown data
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

            // Handle the image upload
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

            // Add the book
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

            // Add relationships to authors
            foreach (var authorId in model.AuthorIds)
            {
                _bookAuthorRepo.Add(new BookAuthor { BookId = book.Id, AuthorId = authorId });
            }

            // Add relationships to categories
            foreach (var categoryId in model.CategoryIds)
            {
                _bookCategoryRepo.Add(new BookCategory { BookId = book.Id, CategoryId = categoryId });
            }

            TempData["Success"] = "Book added successfully!";
            return RedirectToAction("Index");
        }

        [HttpGet]
        public IActionResult EditBook(int id)
        {
            var book = _bookRepo.FirstOrDefault(b => b.Id == id);
            if (book == null)
            {
                TempData["Error"] = "Book not found.";
                return RedirectToAction("Index");
            }

            var model = new EditBookVM
            {
                Id = book.Id,
                Title = book.Title,
                ISBN = book.ISBN,
                Summary = book.Summary,
                PublisherId = book.PublisherId,
                AuthorIds = _bookAuthorRepo.GetAll().Where(ba => ba.BookId == id).Select(ba => ba.AuthorId).ToList(),
                CategoryIds = _bookCategoryRepo.GetAll().Where(bc => bc.BookId == id).Select(bc => bc.CategoryId).ToList(),
                CoverImagePath = book.CoverImagePath
            };

            ViewBag.Authors = _authorRepo.GetAll();
            ViewBag.Categories = _categoryRepo.GetAll();
            ViewBag.Publishers = _publisherRepo.GetAll();

            return View(model);
        }

        [HttpPost]
        public IActionResult EditBook(EditBookVM model)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Invalid input.";
                return RedirectToAction("EditBook", new { id = model.Id });
            }

            var book = _bookRepo.FirstOrDefault(b => b.Id == model.Id);
            if (book == null)
            {
                TempData["Error"] = "Book not found.";
                return RedirectToAction("Index");
            }

            if (model.CoverImage != null)
            {
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(model.CoverImage.FileName);
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images", fileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    model.CoverImage.CopyTo(stream);
                }

                book.CoverImagePath = fileName;
            }

            book.Title = model.Title;
            book.ISBN = model.ISBN;
            book.Summary = model.Summary;
            book.PublisherId = model.PublisherId;
            _bookRepo.Update(book);

            var existingAuthors = _bookAuthorRepo.GetAll().Where(ba => ba.BookId == model.Id).ToList();
            foreach (var existingAuthor in existingAuthors)
            {
                _bookAuthorRepo.Delete(existingAuthor);
            }
            foreach (var authorId in model.AuthorIds)
            {
                _bookAuthorRepo.Add(new BookAuthor { BookId = book.Id, AuthorId = authorId });
            }

            var existingCategories = _bookCategoryRepo.GetAll().Where(bc => bc.BookId == model.Id).ToList();
            foreach (var existingCategory in existingCategories)
            {
                _bookCategoryRepo.Delete(existingCategory);
            }
            foreach (var categoryId in model.CategoryIds)
            {
                _bookCategoryRepo.Add(new BookCategory { BookId = book.Id, CategoryId = categoryId });
            }

            TempData["Success"] = "Book updated successfully!";
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult DeleteBook(int id)
        {
            var book = _bookRepo.FirstOrDefault(b => b.Id == id);
            if (book == null)
            {
                TempData["Error"] = "Book not found.";
                return RedirectToAction("Index");
            }

         
            var authors = _bookAuthorRepo.GetAll().Where(ba => ba.BookId == id).ToList();
            foreach (var author in authors)
            {
                _bookAuthorRepo.Delete(author);
            }

            var categories = _bookCategoryRepo.GetAll().Where(bc => bc.BookId == id).ToList();
            foreach (var category in categories)
            {
                _bookCategoryRepo.Delete(category);
            }

            
            _bookRepo.Delete(book);

            TempData["Success"] = "Book deleted successfully!";
            return RedirectToAction("Index");
        }
    }
}
