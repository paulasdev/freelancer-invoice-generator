using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SoloBill.Models;
using SoloBill.ViewModels;

[Authorize]
public class ApplicationUserController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IWebHostEnvironment _environment;

    public ApplicationUserController(UserManager<ApplicationUser> userManager, IWebHostEnvironment environment)
    {
        _userManager = userManager;
        _environment = environment;
    }

    public async Task<IActionResult> Settings()
    {
        var user = await _userManager.GetUserAsync(User);
        return View(user);
    }

    [HttpPost]
    public async Task<IActionResult> Settings(ApplicationUser model, IFormFile logoFile)
    {
        if (string.IsNullOrWhiteSpace(model.CompanyName))
        {
            ModelState.AddModelError("CompanyName", "Company name is required.");
        }

        var user = await _userManager.GetUserAsync(User);

        if (!ModelState.IsValid)
        {
            model.Email = user.Email;
            model.LogoPath = user.LogoPath;
            return View(model);
        }

        // Update user properties
        user.CompanyName = model.CompanyName;
        user.Address = model.Address;
        user.CompanyPhone = model.CompanyPhone;
        user.BankName = model.BankName;
        user.IBAN = model.IBAN;
        user.BIC = model.BIC;
        user.BankDetails = model.BankDetails;

        // Handle logo upload
        if (logoFile != null && logoFile.Length > 0)
        {
            var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads");
            Directory.CreateDirectory(uploadsFolder);

            var fileExtension = Path.GetExtension(logoFile.FileName);
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };

            if (!allowedExtensions.Contains(fileExtension.ToLower()))
            {
                ModelState.AddModelError("LogoFile", "Only image files (.jpg, .png, .gif) are allowed.");
                model.Email = user.Email;
                model.LogoPath = user.LogoPath;
                return View(model);
            }

            var uniqueFileName = $"{Guid.NewGuid()}{fileExtension}";
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using var stream = new FileStream(filePath, FileMode.Create);
            await logoFile.CopyToAsync(stream);

            user.LogoPath = uniqueFileName;
        }

        //Save changes
        var result = await _userManager.UpdateAsync(user);

        if (result.Succeeded)
        {
            TempData["SuccessMessage"] = "Settings updated successfully!";
            return RedirectToAction("Settings");
        }

        // If errors occurred during update
        foreach (var error in result.Errors)
        {
            ModelState.AddModelError("", error.Description);
        }

        return View(model);
    }

}