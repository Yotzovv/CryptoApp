using CryptoApp.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CryptoApp.API.Controllers;

[Authorize]
[Route("api/[controller]")]
[ApiController]
public class CoinloreController : ControllerBase
{
    private readonly ICoinloreService _coinloreService;
    private readonly ILogService _logService;

    public CoinloreController(ICoinloreService coinloreService, ILogService logService)
    {
        _coinloreService = coinloreService;
        _logService = logService;
    }
    
    [HttpGet("TopCoins")]
    public async Task<IActionResult> GetTopCoins()
    {
        _logService.LogInfo("GetTopCoins endpoint called.");

        try
        {
            var coins = await _coinloreService.FetchCoinsInBatchAsync(0, 100);
            _logService.LogInfo("Successfully fetched top coins from CoinloreService.");

            return Ok(coins);
        }
        catch (Exception ex)
        {
            _logService.LogError("Error occurred while fetching top coins.", ex);
            return StatusCode(500, new { message = "An error occurred while processing your request." });
        }
    }
}