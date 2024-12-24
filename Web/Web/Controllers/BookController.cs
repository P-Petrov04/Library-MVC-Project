using Common.Entities;
using Common.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Web.ViewModels.Books;

namespace Web.Controllers
{
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

        [HttpGet]
        public IActionResult Books(string search, string filterBy)
        {
            var books = _bookRepo.GetAll()
                .Select(b => new Book
                {
                    Id = b.Id,
                    Title = b.Title,
                    CoverImagePath = b.CoverImagePath,
                    ISBN = b.ISBN,
                    Publisher = b.Publisher,
                    BookAuthors = _bookAuthorRepo.GetAll()
                        .Where(ba => ba.BookId == b.Id)
                        .Select(ba => new BookAuthor
                        {
                            Author = _authorRepo.FirstOrDefault(a => a.Id == ba.AuthorId)
                        }).ToList()
                }).ToList();

            if (!string.IsNullOrEmpty(search))
            {
                books = books.Where(b => b.Title.Contains(search, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            if (!string.IsNullOrEmpty(filterBy))
            {
                books = filterBy switch
                {
                    "ISBN" => books.OrderBy(b => b.ISBN).ToList(),
                    "Publisher" => books.OrderBy(b => b.Publisher?.Name).ToList(),
                    _ => books
                };
            }

            return View(books);
        }

        [HttpGet]
        public IActionResult Details(int id)
        {
            var book = _bookRepo.FirstOrDefault(b => b.Id == id);

            if (book == null)
            {
                return NotFound();
            }

            var bookAuthors = _bookAuthorRepo.GetAll()
                .Where(ba => ba.BookId == id)
                .Select(ba => _authorRepo.FirstOrDefault(a => a.Id == ba.AuthorId).Name)
                .ToList();

            var bookCategories = _bookCategoryRepo.GetAll()
                .Where(bc => bc.BookId == id)
                .Select(bc => _categoryRepo.FirstOrDefault(c => c.Id == bc.CategoryId).Name)
                .ToList();

            var model = new
            {
                Id = book.Id,
                Title = book.Title,
                CoverImagePath = book.CoverImagePath,
                Summary = book.Summary,
                Authors = bookAuthors,
                Categories = bookCategories
            };

            return View(model);
        }

        [HttpGet]
        public IActionResult Index()
        {
            if (!IsAuthorized())
            {
                return UnauthorizedRedirect();
            }

            var books = _bookRepo.GetAll()
                .OrderByDescending(b => b.PublishedDate)
                .Take(10)
                .ToList();

            return View(books);
        }

        [HttpGet]
        public IActionResult AddBook()
        {
            if (!IsAuthorized())
            {
                return UnauthorizedRedirect();
            }

            ViewBag.Authors = _authorRepo.GetAll();
            ViewBag.Categories = _categoryRepo.GetAll();
            ViewBag.Publishers = _publisherRepo.GetAll();
            return View();
        }

        [HttpPost]
        public IActionResult AddBook(AddBookVM model)
        {
            if (!IsAuthorized())
            {
                return UnauthorizedRedirect();
            }

            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Invalid input.";
                return RedirectToAction("AddBook");
            }

            string? fileName = null;
            if (model.CoverImage != null && model.CoverImage.Length > 0)
            {
                // Handle image upload logic
                try
                {
                    string[] permittedExtensions = { ".jpg", ".jpeg", ".png", ".gif" };
                    string fileExtension = Path.GetExtension(model.CoverImage.FileName).ToLower();

                    if (!permittedExtensions.Contains(fileExtension))
                    {
                        TempData["Error"] = "Invalid file type. Only .jpg, .jpeg, .png, and .gif are allowed.";
                        return RedirectToAction("AddBook");
                    }

                    fileName = Guid.NewGuid().ToString() + fileExtension;
                    string uploadDirectory = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images");
                    if (!Directory.Exists(uploadDirectory))
                    {
                        Directory.CreateDirectory(uploadDirectory);
                    }

                    string filePath = Path.Combine(uploadDirectory, fileName);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        model.CoverImage.CopyTo(stream);
                    }
                }
                catch (Exception ex)
                {
                    TempData["Error"] = "An error occurred while uploading the image: " + ex.Message;
                    return RedirectToAction("AddBook");
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

            try
            {
                _bookRepo.Add(book);

                foreach (var authorId in model.AuthorIds)
                {
                    _bookAuthorRepo.Add(new BookAuthor { BookId = book.Id, AuthorId = authorId });
                }

                foreach (var categoryId in model.CategoryIds)
                {
                    _bookCategoryRepo.Add(new BookCategory { BookId = book.Id, CategoryId = categoryId });
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = "An error occurred while saving the book: " + ex.Message;
                return RedirectToAction("AddBook");
            }

            TempData["Success"] = "Book added successfully!";
            return RedirectToAction("Index");
        }

        // Other actions like EditBook, DeleteBook follow a similar pattern...
    }
}
