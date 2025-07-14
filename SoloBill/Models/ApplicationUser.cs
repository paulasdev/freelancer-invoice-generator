using Microsoft.AspNetCore.Identity;

namespace SoloBill.Models
{
    public class ApplicationUser : IdentityUser
    {
       
    public string CompanyName { get; set; } = string.Empty;
    public string? Address { get; set; }
    public string? CompanyPhone { get; set; }
    public string? LogoPath { get; set; }
    }
}