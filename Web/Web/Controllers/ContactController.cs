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
            TempData["Success"] = "Thank you for reaching out! We'll get back to you soon.";
            return RedirectToAction("Index");
        }
    }
}
