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
    private readonly ILogService _logService;

    public PortfolioController(IPortfolioService portfolioService, UserManager<AspNetUser> userManager, ICoinloreService coinloreService, ILogService logService)
    {
        _portfolioService = portfolioService;
        _userManager = userManager;
        _coinloreService = coinloreService;
        _logService = logService;
    }
    
    
    [HttpGet("current")]
    public async Task<ActionResult<PortfolioDto>> Get()
    {
        var userId = new Guid(User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value);

        if(userId == null || userId == Guid.Empty)
        {
            _logService.LogError("No user ID claim present in token.");
            
            return Unauthorized("No user ID claim present in token.");
        }
      
        var coinsInfoCache = await _coinloreService.FetchCoinsInBatchAsync(1, 100);
        
        var user = await _userManager.FindByIdAsync(userId.ToString());
        
        _logService.LogInfo($"User {user?.UserName} requested current portfolio.");

        await _portfolioService.CalculateCurrentPortfolioValue(user!, coinsInfoCache);
        
        var portfolio = await _portfolioService.Get(userId);
        
        _logService.LogInfo("Current portfolio returned.");
        
        return Ok(portfolio);
    }

    [HttpPatch("update-portfolio")]
    public async Task<IActionResult> UpdatePortfolio()
    {
        _logService.LogInfo("Updating portfolio..");
        
        var userId = new Guid(User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value);

        if(userId == null || userId == Guid.Empty)
        {
            _logService.LogError("No user ID claim present in token.");
            return Unauthorized("No user ID claim present in token.");
        }
        
        var user = await _userManager.FindByIdAsync(userId.ToString());
        
        var coinsInfoCache = await _coinloreService.FetchCoinsInBatchAsync(0, 100);

        try
        {
            await _portfolioService.UpdatePortfolio(user!, coinsInfoCache);
            _logService.LogInfo("Portfolio updated successfully.");
        }
        catch (Exception e)
        {
            _logService.LogError("Error occurred while updating portfolio.", e);
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
            _logService.LogError("No user ID claim present in token.");

            return Unauthorized("No user ID claim present in token.");
        }

        if (dto.File.Length == 0)
        {
            _logService.LogError("Invalid file.");   
            return BadRequest(new { message = "Invalid file." });
        }

        if (!dto.File.ContentType.Equals("text/plain", StringComparison.OrdinalIgnoreCase))
        {
            _logService.LogError("Invalid file type. Only text files are allowed.");
            return BadRequest(new { message = "Invalid file type. Only text files are allowed." });
        }
        
        try
        {
            var user = await _userManager.FindByIdAsync(userId);
            
            await _portfolioService.Upload(dto.File, user!);
            
            _logService.LogInfo($"User {user?.UserName} uploaded a file.");
            
            var coinsInfoCache = await _coinloreService.FetchCoinsInBatchAsync(1, 100);
            
            _logService.LogInfo("Calculating initial portfolio value..");
            await _portfolioService.CalculateInitialPortfolioValue(user!);
            
            _logService.LogInfo("Calculating current portfolio value..");
            await _portfolioService.CalculateCurrentPortfolioValue(user!, coinsInfoCache); // current
            
            _logService.LogInfo("File uploaded successfully.");
            
            return Ok();
        }
        catch (Exception e)
        {
            _logService.LogError("Error occurred while uploading file.", e);
            return StatusCode(500, new { message = e.Message });
        }
    }
}