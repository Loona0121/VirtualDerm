using Microsoft.AspNetCore.Mvc;

namespace DermaAI.Controllers
{
    public class ProfileController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}