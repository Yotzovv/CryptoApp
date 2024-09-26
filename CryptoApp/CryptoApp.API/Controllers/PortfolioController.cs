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
    private readonly ICoinloreService _coinloreService;

    public PortfolioController(IPortfolioService portfolioService, UserManager<AspNetUser> userManager, ICoinloreService coinloreService)
    {
        _portfolioService = portfolioService;
        _userManager = userManager;
        _coinloreService = coinloreService;
    }
    
    
    [HttpGet("current")]
    public async Task<ActionResult<PortfolioDto>> Get()
    {
        var userId = new Guid(User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value);

        if(userId == null || userId == Guid.Empty)
        {
            return Unauthorized("No user ID claim present in token.");
        }
      
        var coinsInfoCache = await _coinloreService.FetchCoinsInBatchAsync(1, 100);
        
        var user = await _userManager.FindByIdAsync(userId.ToString());

        // await _portfolioService.CalculateCurrentPortfolioValue(user!, coinsInfoCache);
        
        var portfolio = await _portfolioService.Get(userId);
        
        return Ok(portfolio);
    }

    [HttpPatch("update-portfolio")]
    public async Task<IActionResult> UpdatePortfolio()
    {
        var userId = new Guid(User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value);

        if(userId == null || userId == Guid.Empty)
        {
            return Unauthorized("No user ID claim present in token.");
        }
        
        var user = await _userManager.FindByIdAsync(userId.ToString());
        
        var coinsInfoCache = await _coinloreService.FetchCoinsInBatchAsync(0, 100);

        try
        {
            await _portfolioService.UpdatePortfolio(user!, coinsInfoCache);
        }
        catch (Exception e)
        {
            return BadRequest();
        }

        return Ok();
    }
    
    [HttpPost("upload")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> Post([FromForm] FileUploadDto dto)
    {
        var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

        if(userId == null)
        {
            return Unauthorized("No user ID claim present in token.");
        }
        
        if (dto.File.Length == 0)
            return BadRequest(new { message = "Invalid file." });
        
        if (!dto.File.ContentType.Equals("text/plain", StringComparison.OrdinalIgnoreCase))
            return BadRequest(new { message = "Invalid file type. Only text files are allowed." });
        
        try
        {
            var user = await _userManager.FindByIdAsync(userId);
            
            await _portfolioService.Upload(dto.File, user!);
            
            var coinsInfoCache = await _coinloreService.FetchCoinsInBatchAsync(1, 100);
            
            await _portfolioService.CalculateInitialPortfolioValue(user!);
            // await _portfolioService.CalculateCurrentPortfolioValue(user!, coinsInfoCache); // current
            
            return Ok();
        }
        catch (Exception e)
        {
            return StatusCode(500, new { message = "An error occurred while processing the file." });
        }
    }
}