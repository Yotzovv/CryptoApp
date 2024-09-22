using System.Security.Claims;
using CryptoApp.Data.dtos;
using CryptoApp.Data.Models;
using CryptoApp.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace CryptoApp.API.Controllers;

[Authorize]
[Route("api/[controller]")]
[ApiController]
public class PortfolioController : ControllerBase
{
    private readonly IPortfolioService _portfolioService;
    private readonly UserManager<AspNetUser> _userManager;

    public PortfolioController(IPortfolioService portfolioService, UserManager<AspNetUser> userManager)
    {
        _portfolioService = portfolioService;
        _userManager = userManager;
    }
    
    
    [HttpGet("current")]
    public IActionResult Get()
    {
        return Ok("Hello World");
    }
    
    [HttpPost("upload")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> Post([FromForm] FileUploadDto dto)
    {
        if (dto.File.Length == 0)
            return BadRequest(new { message = "Invalid file." });
        
        if (!dto.File.ContentType.Equals("text/plain", StringComparison.OrdinalIgnoreCase))
            return BadRequest(new { message = "Invalid file type. Only text files are allowed." });
        
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            
            var user = await _userManager.FindByIdAsync(userId ?? throw new InvalidOperationException());
            
            await _portfolioService.Upload(dto.File, user);
            return Ok();
        }
        catch (Exception e)
        {
            return StatusCode(500, new { message = "An error occurred while processing the file." });
        }
    }
}