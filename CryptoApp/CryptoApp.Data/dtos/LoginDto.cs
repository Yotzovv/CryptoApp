using System.ComponentModel.DataAnnotations;

namespace CryptoApp.Data.dtos;

public class LoginDto
{
    [Required]
    [EmailAddress]
    public string Email { get; set; }
    
    [Required]
    public string Password { get; set; }
}