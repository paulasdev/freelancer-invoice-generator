using Microsoft.AspNetCore.Mvc;

[Route("Error")]
public class ErrorController : Controller
{
    // Handles /Error/404, /Error/401, etc.
    [Route("{statusCode:int}")]
    public IActionResult HttpStatusCodeHandler(int statusCode)
        => statusCode == 404 ? View("Error404") : View("Error");

    // Handles /Error/500 from UseExceptionHandler
    [Route("500")]
    public IActionResult Error500() => View("Error500");
}