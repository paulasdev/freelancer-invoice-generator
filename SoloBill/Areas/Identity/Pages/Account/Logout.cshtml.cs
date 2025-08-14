using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SoloBill.Models;

namespace SoloBill.Areas.Identity.Pages.Account
{
    public class LogoutModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;

        public LogoutModel(SignInManager<ApplicationUser> signInManager)
        {
            _signInManager = signInManager;
        }

        public async Task<IActionResult> OnGet()
        {
            await _signInManager.SignOutAsync();
            return RedirectToPage("/Account/Login"); // 👈 redirect after logout
        }

        public async Task<IActionResult> OnPost(string returnUrl = null)
        {
            await _signInManager.SignOutAsync();
            return RedirectToPage("/Account/Login"); // 👈 also redirect after form POST logout
        }
    }
}