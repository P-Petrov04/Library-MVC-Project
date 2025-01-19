using Microsoft.AspNetCore.Mvc;

namespace Web.Controllers
{
    public class ContactController : Controller
    {
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Submit(string name, string email, string message)
        {
            // Perform any logic you want with the submitted data
            // (e.g., store in the database, send an email, etc.)

            // Example: Just display a success message for now
            TempData["Success"] = "Thank you for reaching out! We'll get back to you soon.";
            return RedirectToAction("Index");
        }
    }
}
