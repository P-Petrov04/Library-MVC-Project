using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Web.Controllers
{
    [Authorize(Roles = "Moderator")]
    public class ModeratorController : Controller
    {
        public IActionResult Index()
        {
            // Display a list of books
            return View();
        }

        [HttpGet]
        public IActionResult AddBook()
        {
            return View();
        }

        [HttpPost]
        public IActionResult AddBook(string title, string author, string isbn)
        {
            // Add book logic
            TempData["Success"] = "Book successfully added!";
            return RedirectToAction("Index");
        }

        [HttpGet]
        public IActionResult EditBook(int id)
        {
            // Fetch the book by ID
            return View();
        }

        [HttpPost]
        public IActionResult EditBook(int id, string title, string author, string isbn)
        {
            // Update the book
            TempData["Success"] = "Book successfully updated!";
            return RedirectToAction("Index");
        }

        public IActionResult DeleteBook(int id)
        {
            // Delete the book logic
            TempData["Success"] = "Book successfully deleted!";
            return RedirectToAction("Index");
        }
    }
}
