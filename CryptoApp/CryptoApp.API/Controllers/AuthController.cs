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

        return Ok(new { token });

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
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Email!),
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
        };

        if (_configuration["JwtIssuer"] == null || _configuration["JwtKey"] == null)
            throw new Exception("JwtIssuer or JwtKey is missing from app settings."); // TODO: Is it ok to throw exception in controller?

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JwtKey"]!));
        
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            _configuration["JwtIssuer"],
            _configuration["JwtIssuer"],
            claims,
            expires: DateTime.Now.AddDays(7),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}