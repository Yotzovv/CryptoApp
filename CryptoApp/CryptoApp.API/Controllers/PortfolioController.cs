using CryptoApp.Data.dtos;
using CryptoApp.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CryptoApp.API.Controllers;

// [Authorize]
[Route("api/[controller]")]
[ApiController]
public class PortfolioController : ControllerBase
{
    private readonly IPortfolioService _portfolioService;
    
    public PortfolioController(IPortfolioService portfolioService)
    {
        _portfolioService = portfolioService;
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
            await _portfolioService.Upload(dto.File);
            return Ok();
        }
        catch (Exception e)
        {
            return StatusCode(500, new { message = "An error occurred while processing the file." });
        }
    }
}