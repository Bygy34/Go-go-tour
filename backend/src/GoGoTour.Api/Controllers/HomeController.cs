using Microsoft.AspNetCore.Mvc;

namespace GoGoTour.Api.Controllers;

public class HomeController : Controller
{
    public IActionResult Index()
    {
        return RedirectToAction("Index", "ToursPage");
    }
}
