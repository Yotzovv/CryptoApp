using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using CryptoApp.Data.dtos;
using CryptoApp.Data.Models;
using CryptoApp.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace CryptoApp.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly UserManager<AspNetUser> _userManager;
    private readonly SignInManager<AspNetUser> _signInManager;
    private readonly IConfiguration _configuration;
    private readonly ILogService _logService;

    public AuthController(UserManager<AspNetUser> userManager, SignInManager<AspNetUser> signInManager, IConfiguration configuration, ILogService logService)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _configuration = configuration;
        _logService = logService;
    }
    
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto model)
    {
        _logService.LogInfo($"Login attempt for user: {model.Email}");

        var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, false, false);

        if (!result.Succeeded) 
        {
            _logService.LogError($"Login failed for user: {model.Email}");
            return Unauthorized(new { message = "Invalid login attempt." });
        }
        
        var user = await _userManager.FindByEmailAsync(model.Email);
        
        var token = GenerateJwtToken(user!);

        _logService.LogInfo($"Login successful for user: {model.Email}");

        return Ok(new AuthResponse { Token = token });
    }
    
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] LoginDto model)
    {
        _logService.LogInfo($"Registration attempt for user: {model.Email}");

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
                _logService.LogInfo($"Registration successful for user: {model.Email}");
                return Ok(new { message = "Registration successful." });
            }

            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            _logService.LogError($"Registration failed for user: {model.Email}. Errors: {errors}");
            
            return BadRequest(new { message = "Registration failed.", errors = result.Errors });
        }
        catch (Exception e)
        {
            _logService.LogError($"Exception during registration for user: {model.Email}", e);
            throw;
        }
    }
    
    private string GenerateJwtToken(AspNetUser user)
    {
        _logService.LogInfo($"Generating JWT token for user: {user.Email}");

        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Key"]);
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new Claim[]
            {
                new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            }),
            Expires = DateTime.UtcNow.AddHours(1),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
            Issuer = _configuration["Jwt:Issuer"],
            Audience = _configuration["Jwt:Audience"] 
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        var tokenString = tokenHandler.WriteToken(token);

        _logService.LogInfo($"JWT token generated for user: {user.Email}");

        return tokenString;
    }
}