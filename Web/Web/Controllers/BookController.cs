using Common.Entities;
using Common.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Web.ViewModels.Books;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;


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
        private readonly BaseRepository<Review> _reviewRepo;
        private readonly BaseRepository<User> _userRepo;
        private readonly BaseRepository<Loan> _loanRepo;




        public BookController(
            BaseRepository<Book> bookRepo,
            BaseRepository<Author> authorRepo,
            BaseRepository<BookAuthor> bookAuthorRepo,
            BaseRepository<BookCategory> bookCategoryRepo,
            BaseRepository<Category> categoryRepo,
            BaseRepository<Publisher> publisherRepo,
            BaseRepository<Review> reviewRepo,
            BaseRepository<User> userRepo,
            BaseRepository<Loan> loanRepo
            )
        {
            _bookRepo = bookRepo;
            _authorRepo = authorRepo;
            _bookAuthorRepo = bookAuthorRepo;
            _bookCategoryRepo = bookCategoryRepo;
            _categoryRepo = categoryRepo;
            _publisherRepo = publisherRepo;
            _reviewRepo = reviewRepo;
            _userRepo = userRepo;
            _loanRepo = loanRepo;
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


        [HttpPost]
        public IActionResult AddReview(int bookId, string comment, int rating)
        {
            // Get the logged-in user's ID and username from the session
            var userId = HttpContext.Session.GetString("loggedUserId");

            if (string.IsNullOrEmpty(userId))
            {
                TempData["Error"] = "You need to be logged in to add a review.";
                return RedirectToAction("Details", new { id = bookId });
            }

            if (rating < 1 || rating > 5 || string.IsNullOrWhiteSpace(comment))
            {
                TempData["Error"] = "Invalid review data.";
                return RedirectToAction("Details", new { id = bookId });
            }

            var review = new Review
            {
                BookId = bookId,
                UserId = int.Parse(userId),
                Rating = rating,
                Comment = comment
            };

            try
            {
                _reviewRepo.Add(review);
                TempData["Success"] = "Review added successfully!";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "An error occurred while adding the review: " + ex.Message;
            }

            return RedirectToAction("Details", new { id = bookId });
        }


        [HttpPost]
        public IActionResult DeleteReview(int reviewId)
        {
            var userId = HttpContext.Session.GetString("loggedUserId");
            var userRole = HttpContext.Session.GetInt32("UserRole");

            if (string.IsNullOrEmpty(userId))
            {
                TempData["Error"] = "You need to be logged in to delete a review.";
                return RedirectToAction("Details", "Book", new { id = reviewId });
            }

            if (!IsAuthorizedToDelete(userRole))
            {
                TempData["Error"] = "You are not authorized to delete a review.";
                return RedirectToAction("Details", "Book", new { id = reviewId });
            }

            var review = _reviewRepo.FirstOrDefault(r => r.Id == reviewId);

            if (review == null)
            {
                TempData["Error"] = "Review not found.";
                return RedirectToAction("Details", "Book", new { id = reviewId });
            }

            try
            {
                _reviewRepo.Delete(review);
                TempData["Success"] = "Review deleted successfully!";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "An error occurred while deleting the review: " + ex.Message;
            }

            return RedirectToAction("Details", "Book", new { id = review.BookId });
        }


        private bool IsAuthorizedToDelete(int? userRole)
        {
            return userRole == 1 || userRole == 2; // Admin (1) or Moderator (2)
        }

        public IActionResult Books(string search, string filterBy, string statusFilter)
        {
            var books = _bookRepo.GetAll()
                .Select(b => new
                {
                    Book = b,
                    LatestLoan = _loanRepo.GetAll()
                        .Where(l => l.BookId == b.Id)
                        .OrderByDescending(l => l.Id)
                        .FirstOrDefault()
                })
                .Select(b => new
                {
                    b.Book.Id,
                    b.Book.Title,
                    b.Book.CoverImagePath,
                    b.Book.ISBN,
                    b.Book.Publisher,
                    IsAvailable = b.LatestLoan == null || b.LatestLoan.ReturnDate.HasValue,
                    BookAuthors = _bookAuthorRepo.GetAll()
                        .Where(ba => ba.BookId == b.Book.Id)
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

            if (!string.IsNullOrEmpty(statusFilter))
            {
                books = books.Where(b =>
                    (statusFilter == "Available" && b.IsAvailable) ||
                    (statusFilter == "Unavailable" && !b.IsAvailable)
                ).ToList();
            }

            return View(books.Select(b => new Book
            {
                Id = b.Id,
                Title = b.Title,
                CoverImagePath = b.CoverImagePath,
                ISBN = b.ISBN,
                Publisher = b.Publisher,
                BookAuthors = b.BookAuthors
            }).ToList());
        }



        private bool IsBookAvailable(Book book)
        {
            var lastLoan = _loanRepo.GetAll()
                .Where(l => l.BookId == book.Id)
                .OrderByDescending(l => l.LoanDate)
                .FirstOrDefault();

            if (lastLoan == null || lastLoan.ReturnDate == null || lastLoan.ReturnDate.ToString() == "N/A")
            {
                return false; // Assuming that if ReturnDate is "N/A", the book is unavailable.
            }

            return lastLoan.ReturnDate >= DateTime.Today;
        }



        [HttpGet]
        public IActionResult Details(int id)
        {
            var book = _bookRepo.FirstOrDefault(b => b.Id == id);

            if (book == null)
            {
                return NotFound();
            }

            var reviews = _reviewRepo.GetAll()
            .Where(r => r.BookId == id)
            .Select(r => new
            {
                r.Id,
                r.Comment,
                r.Rating,
                UserName = GetUserNameById(r.UserId) // Fetch username for each review
            })
            .ToList();

            var loans = _loanRepo.GetAll()
                .Where(l => l.BookId == id)
                .OrderBy(l => l.LoanDate)
                .ToList();

            var loanDates = loans.Select(l => new
            {
                LoanDate = l.LoanDate.ToString("yyyy-MM-dd"),
                DueDate = l.DueDate.ToString("yyyy-MM-dd"),
                ReturnDate = l.ReturnDate?.ToString("yyyy-MM-dd") ?? "N/A",
                UserName = GetUserNameById(l.UserId)
            }).ToList();

            var bookAuthors = _bookAuthorRepo.GetAll()
                .Where(ba => ba.BookId == id)
                .Select(ba => _authorRepo.FirstOrDefault(a => a.Id == ba.AuthorId).Name)
                .ToList();

            var bookCategories = _bookCategoryRepo.GetAll()
                .Where(bc => bc.BookId == id)
                .Select(bc => _categoryRepo.FirstOrDefault(c => c.Id == bc.CategoryId).Name)
                .ToList();

            ViewBag.UserRole = HttpContext.Session.GetInt32("UserRole");
            ViewBag.UserId = HttpContext.Session.GetString("loggedUserId");
            ViewBag.LoanDates = loanDates;

            var model = new
            {
                Id = book.Id,
                Title = book.Title,
                CoverImagePath = book.CoverImagePath,
                Summary = book.Summary,
                Authors = bookAuthors,
                Categories = bookCategories,
                Reviews = reviews,
                Loans = loans
            };

            return View(model);
        }


        private string GetUserNameById(int userId)
        {
            // Fetch the username from the user repository or database
            var user = _userRepo.FirstOrDefault(u => u.Id == userId);
            return user != null ? user.Name : "Unknown User";
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

            TempData["Success"] = $"Book '{book.Title}' added successfully!";
            return RedirectToAction("Index");
        }

        // Other actions like EditBook, DeleteBook follow a similar pattern...

        [HttpGet]
        public IActionResult EditBook(int id)
        {
            if (!IsAuthorized())
            {
                return UnauthorizedRedirect();
            }

            var book = _bookRepo.FirstOrDefault(b => b.Id == id);

            if (book == null)
            {
                return NotFound();
            }

            var model = new EditBookVM
            {
                Id = book.Id,
                Title = book.Title,
                ISBN = book.ISBN,
                Summary = book.Summary,
                PublisherId = book.PublisherId,
                PublishedDate = book.PublishedDate,
                AuthorIds = _bookAuthorRepo.GetAll()
                    .Where(ba => ba.BookId == id)
                    .Select(ba => ba.AuthorId)
                    .ToList(),
                CategoryIds = _bookCategoryRepo.GetAll()
                    .Where(bc => bc.BookId == id)
                    .Select(bc => bc.CategoryId)
                    .ToList()
            };

            ViewBag.Authors = _authorRepo.GetAll();
            ViewBag.Categories = _categoryRepo.GetAll();
            ViewBag.Publishers = _publisherRepo.GetAll();

            return View(model);
        }

        [HttpPost]
        public IActionResult EditBook(EditBookVM model)
        {
            if (!IsAuthorized())
            {
                return UnauthorizedRedirect();
            }

            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Invalid input.";
                return RedirectToAction("EditBook", new { id = model.Id });
            }

            string? fileName = null;
            if (model.CoverImage != null && model.CoverImage.Length > 0)
            {
                try
                {
                    string[] permittedExtensions = { ".jpg", ".jpeg", ".png", ".gif" };
                    string fileExtension = Path.GetExtension(model.CoverImage.FileName).ToLower();

                    if (!permittedExtensions.Contains(fileExtension))
                    {
                        TempData["Error"] = "Invalid file type. Only .jpg, .jpeg, .png, and .gif are allowed.";
                        return RedirectToAction("EditBook", new { id = model.Id });
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
                    return RedirectToAction("EditBook", new { id = model.Id });
                }
            }

            var book = _bookRepo.FirstOrDefault(b => b.Id == model.Id);

            if (book == null)
            {
                return NotFound();
            }

            book.Title = model.Title;
            book.ISBN = model.ISBN;
            book.Summary = model.Summary;
            book.PublisherId = model.PublisherId;
            book.PublishedDate = model.PublishedDate;
            book.CoverImagePath = fileName ?? book.CoverImagePath;

            try
            {
                _bookRepo.Update(book);

                var existingAuthors = _bookAuthorRepo.GetAll().Where(ba => ba.BookId == model.Id).ToList();
                foreach (var author in existingAuthors)
                {
                    _bookAuthorRepo.Delete(author);
                }

                foreach (var authorId in model.AuthorIds)
                {
                    _bookAuthorRepo.Add(new BookAuthor { BookId = model.Id, AuthorId = authorId });
                }

                var existingCategories = _bookCategoryRepo.GetAll().Where(bc => bc.BookId == model.Id).ToList();
                foreach (var category in existingCategories)
                {
                    _bookCategoryRepo.Delete(category);
                }

                foreach (var categoryId in model.CategoryIds)
                {
                    _bookCategoryRepo.Add(new BookCategory { BookId = model.Id, CategoryId = categoryId });
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = "An error occurred while updating the book: " + ex.Message;
                return RedirectToAction("EditBook", new { id = model.Id });
            }

            TempData["Success"] = $"Book '{book.Title}' edited successfully!";
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult DeleteBook(int id)
        {
            if (!IsAuthorized())
            {
                return UnauthorizedRedirect();
            }

            var book = _bookRepo.FirstOrDefault(b => b.Id == id);

            if (book == null)
            {
                TempData["Error"] = "Book not found.";
                return RedirectToAction("Index");
            }

            try
            {
                // Delete related authors
                var bookAuthors = _bookAuthorRepo.GetAll().Where(ba => ba.BookId == id).ToList();
                foreach (var bookAuthor in bookAuthors)
                {
                    _bookAuthorRepo.Delete(bookAuthor);
                }

                // Delete related categories
                var bookCategories = _bookCategoryRepo.GetAll().Where(bc => bc.BookId == id).ToList();
                foreach (var bookCategory in bookCategories)
                {
                    _bookCategoryRepo.Delete(bookCategory);
                }

                // Delete the book
                _bookRepo.Delete(book);

                // Optionally delete the cover image from the file system
                if (!string.IsNullOrEmpty(book.CoverImagePath))
                {
                    string filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images", book.CoverImagePath);
                    if (System.IO.File.Exists(filePath))
                    {
                        System.IO.File.Delete(filePath);
                    }
                }

                TempData["Success"] = $"Book '{book.Title}' deleted successfully!";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "An error occurred while deleting the book: " + ex.Message;
            }

            return RedirectToAction("Index");
        }


    }
}
