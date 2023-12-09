using Microsoft.AspNetCore.Mvc;

namespace NaeTime.Client.WEBAPI.Controllers;

[Route("[controller]")]
public class ConfigurationController : Controller
{
    [HttpGet("available")]
    public IActionResult IsServiceAvailable()
    {
        return Ok();
    }
}
