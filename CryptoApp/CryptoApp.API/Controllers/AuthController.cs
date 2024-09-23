using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using CryptoApp.Data.dtos;
using CryptoApp.Data.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace CryptoApp.API.Controllers;

public class AuthController : ControllerBase
{
    private readonly UserManager<AspNetUser> _userManager;
    private readonly SignInManager<AspNetUser> _signInManager;
    private readonly IConfiguration _configuration;

    public AuthController(UserManager<AspNetUser> userManager, SignInManager<AspNetUser> signInManager, IConfiguration configuration)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _configuration = configuration;
    }
    
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto model)
    {
        var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, false, false);

        if (!result.Succeeded) 
            return Unauthorized(new { message = "Invalid login attempt." });
        
        var user = await _userManager.FindByEmailAsync(model.Email);
        
        var token = GenerateJwtToken(user!);

        return Ok(new AuthResponse { Token = token });

    }
    
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] LoginDto model)
    {
        var user = new AspNetUser
        {
            UserName = model.Email,
            Email = model.Email,
            IsActive = true,
        };

        user.Portfolio = new Portfolio()
        {
            User = user,
        };
        
        try
        {
            var result = await _userManager.CreateAsync(user, model.Password);
            
            if (result.Succeeded)
            {
                return Ok(new { message = "Registration successful." });
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
        
        return BadRequest();
    }
    
    private string GenerateJwtToken(AspNetUser user)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Key"]);
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                // Add more claims as needed
            }),
            Expires = DateTime.UtcNow.AddHours(1),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
            Issuer = _configuration["Jwt:Issuer"],
            Audience = _configuration["Jwt:Audience"] 
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
}