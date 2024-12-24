using Common.Entities;
using Common.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Web.ViewModels.Loans;
using Web.ViewModels.Publishers;

namespace Web.Controllers
{
    public class LoanController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly BaseRepository<Book> _bookRepo;
        private readonly BaseRepository<Loan> _loanRepo;

        public LoanController(UserManager<IdentityUser> userManager, BaseRepository<Book> bookRepo, BaseRepository<Loan> loanRepo)
        {
            _userManager = userManager;
            _bookRepo = bookRepo;
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

        public IActionResult Index()
        {
            if (!IsAuthorized())
            {
                return UnauthorizedRedirect();
            }

            LoanVM model = new LoanVM()
            {
                Items = _loanRepo.GetAll().Where(l => l.ReturnDate == null)
            };
            return View(model);
        }

        public IActionResult CurrentUserLoans()
        {
            // Get the logged-in user's ID from the session or authentication
            var loggedUserId = HttpContext.Session.GetString("loggedUserId");

            if (string.IsNullOrEmpty(loggedUserId))
            {
                TempData["Error"] = "You need to log in to view your loans.";
                return RedirectToAction("Index", "Home"); // Redirect to home or login page
            }

            int userId = int.Parse(loggedUserId);

            // Retrieve loans for the logged-in user
            var userLoans = _loanRepo.GetAll()
                .Where(l => l.UserId == userId);

            return View(userLoans);
        }


        [HttpPost]
        public async Task<IActionResult> MakeLoan(int bookId, DateTime loanDate, DateTime dueDate)
        {
            if(loanDate < DateTime.Now.Date || dueDate < DateTime.Now.Date)
            {
                TempData["Error"] = "Loan date and due date must be after current date.";
                return RedirectToAction("Details", "Book", new { id = bookId });
            }

            if (loanDate >= dueDate)
            {
                TempData["Error"] = "Loan date must be earlier than due date.";
                return RedirectToAction("Details", "Book", new { id = bookId });
            }

            var userId = HttpContext.Session.GetString("loggedUserId");
            if (userId == null)
            {
                TempData["Error"] = "User is not logged!";
                return RedirectToAction("Login", "Auth");
            }

            // Check if the book is available in the selected period
            var isBookAvailable = !_loanRepo.GetAll().Any(loan =>
                loan.BookId == bookId &&
                (
                    (loan.ReturnDate == null && loan.LoanDate <= dueDate && loan.DueDate >= loanDate) || // Active loan conflicts
                    (loan.ReturnDate != null && loan.LoanDate <= dueDate && loan.ReturnDate >= loanDate) // Overlapping loan period
                ));

            if (!isBookAvailable)
            {
                TempData["Error"] = "The book is not available for the selected period.";
                return RedirectToAction("Details", "Book", new { id = bookId });
            }

            // Create a new loan
            var newLoan = new Loan
            {
                BookId = bookId,
                UserId = int.Parse(userId),
                LoanDate = loanDate,
                DueDate = dueDate
            };

            _loanRepo.Add(newLoan);

            TempData["Success"] = "The book has been successfully loaned.";
            return RedirectToAction("CurrentUserLoans", "Loan");
        }


        [HttpPost]
        public async Task<IActionResult> ReturnBook(int id)
        {
            if (!IsAuthorized())
            {
                return UnauthorizedRedirect();
            }

            Loan curr = _loanRepo.FirstOrDefault(l => l.Id == id);
            curr.ReturnDate = DateTime.Now;

            _loanRepo.Update(curr);
            return RedirectToAction("Index");
        }
    }
}
