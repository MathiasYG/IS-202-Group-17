using Microsoft.AspNetCore.Mvc;
using WebApplicationInAspire.Models;

namespace WebApplicationInAspire.Controllers;

public class ResourceController : Controller
{
    private readonly ILogger<ResourceController> _logger;

    public ResourceController(ILogger<ResourceController> logger)
    {
        _logger = logger;
    }

    [HttpGet]
    public IActionResult Register()
    {
        var model = new ResourceViewModel
        {
            Latitude = 58.1467,
            Longitude = 7.9956
        };

        return View(model);
    }

    [HttpPost]
    public IActionResult Register(ResourceViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        _logger.LogInformation("Ressurs registrert: {Name}", model.Name);
        return View("Overview", model);
    }
}