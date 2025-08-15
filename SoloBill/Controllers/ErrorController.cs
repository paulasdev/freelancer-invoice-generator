using Microsoft.AspNetCore.Mvc;

public class ErrorController : Controller
{
    [Route("Error/{statusCode}")]
    public IActionResult HttpStatusCodeHandler(int statusCode)
    {
        Response.StatusCode = statusCode;
        return statusCode == 404 ? View("Error404") : View("Error");
    }

    [Route("Error/500")]
    public IActionResult Error500()
    {
        Response.StatusCode = 500;
        return View("Error500");
    }
}