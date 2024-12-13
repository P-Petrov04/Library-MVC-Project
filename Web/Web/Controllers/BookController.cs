using Common.Entities;
using Common.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Web.ViewModels.Books;

namespace Web.Controllers
{
    //[Authorize(Roles = "Admin")] // Restrict access to Admins (optional)
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
            if (model.CoverImage != null && model.CoverImage.Length > 0)
            {
                try
                {
                    // Validate file extension
                    string[] permittedExtensions = { ".jpg", ".jpeg", ".png", ".gif" };
                    string fileExtension = Path.GetExtension(model.CoverImage.FileName).ToLower();

                    if (!permittedExtensions.Contains(fileExtension))
                    {
                        TempData["Error"] = "Invalid file type. Only .jpg, .jpeg, .png, and .gif are allowed.";
                        return RedirectToAction("AddBook");
                    }

                    // Generate a unique file name with the same extension as the uploaded file
                    fileName = Guid.NewGuid().ToString() + fileExtension;

                    // Define the target directory
                    string uploadDirectory = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images");

                    // Ensure the directory exists
                    if (!Directory.Exists(uploadDirectory))
                    {
                        Directory.CreateDirectory(uploadDirectory);
                    }

                    // Combine the directory and file name to get the full path
                    string filePath = Path.Combine(uploadDirectory, fileName);

                    // Copy the file to the target location
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

            // Add the book
            var book = new Book
            {
                Title = model.Title,
                ISBN = model.ISBN,
                Summary = model.Summary,
                PublisherId = model.PublisherId,
                CoverImagePath = fileName, // null if no image uploaded
                PublishedDate = model.PublishedDate
            };

            try
            {
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
            }
            catch (Exception ex)
            {
                TempData["Error"] = "An error occurred while saving the book: " + ex.Message;
                return RedirectToAction("AddBook");
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
                CoverImagePath = book.CoverImagePath,
                PublishedDate = book.PublishedDate
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

            // Retrieve the book to be edited
            var book = _bookRepo.FirstOrDefault(b => b.Id == model.Id);
            if (book == null)
            {
                TempData["Error"] = "Book not found.";
                return RedirectToAction("Index");
            }

            // Handle image upload if a new image is provided
            if (model.CoverImage != null && model.CoverImage.Length > 0)
            {
                if (!string.IsNullOrEmpty(book.CoverImagePath))
                {
                    string imagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images", book.CoverImagePath);
                    if (System.IO.File.Exists(imagePath))
                    {
                        System.IO.File.Delete(imagePath);
                    }
                }

                string? fileName = null;
                if (model.CoverImage != null && model.CoverImage.Length > 0)
                {
                    try
                    {
                        // Validate file extension
                        string[] permittedExtensions = { ".jpg", ".jpeg", ".png", ".gif" };
                        string fileExtension = Path.GetExtension(model.CoverImage.FileName).ToLower();

                        if (!permittedExtensions.Contains(fileExtension))
                        {
                            TempData["Error"] = "Invalid file type. Only .jpg, .jpeg, .png, and .gif are allowed.";
                            return RedirectToAction("AddBook");
                        }

                        // Generate a unique file name with the same extension as the uploaded file
                        fileName = Guid.NewGuid().ToString() + fileExtension;

                        // Define the target directory
                        string uploadDirectory = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images");

                        // Ensure the directory exists
                        if (!Directory.Exists(uploadDirectory))
                        {
                            Directory.CreateDirectory(uploadDirectory);
                        }

                        // Combine the directory and file name to get the full path
                        string filePath = Path.Combine(uploadDirectory, fileName);

                        // Copy the file to the target location
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
                book.CoverImagePath = fileName;
            }

            // Update book properties
            book.Title = model.Title;
            book.ISBN = model.ISBN;
            book.Summary = model.Summary;
            book.PublisherId = model.PublisherId;
            book.PublishedDate = model.PublishedDate;

            // Update the book record
            _bookRepo.Update(book);

            // Update authors
            var existingAuthors = _bookAuthorRepo.GetAll().Where(ba => ba.BookId == model.Id).ToList();
            foreach (var existingAuthor in existingAuthors)
            {
                _bookAuthorRepo.Delete(existingAuthor);
            }
            foreach (var authorId in model.AuthorIds)
            {
                _bookAuthorRepo.Add(new BookAuthor { BookId = book.Id, AuthorId = authorId });
            }

            // Update categories
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

            if (!string.IsNullOrEmpty(book.CoverImagePath))
            {
                string imagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images", book.CoverImagePath);
                if (System.IO.File.Exists(imagePath))
                {
                    System.IO.File.Delete(imagePath);
                }
            }

            _bookRepo.Delete(book);

            TempData["Success"] = "Book deleted successfully!";
            return RedirectToAction("Index");
        }
    }
}
