using Microsoft.AspNetCore.Mvc;

namespace NaeTime.Client.WEBAPI.Controllers;
[Route("[controller]")]
public class FlyingSessionController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}
