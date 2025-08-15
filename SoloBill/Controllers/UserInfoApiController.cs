using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SoloBill.Models;
using System.Threading.Tasks;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class UserInfoApiController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;

    public UserInfoApiController(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
            return NotFound();

        return Ok(new
        {
            user.Email,
            user.CompanyName,
            user.Address,
            user.CompanyPhone,
            user.LogoPath
        });
    }
}