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

    public CoinloreController(ICoinloreService coinloreService)
    {
        _coinloreService = coinloreService;
    }
    
    [HttpGet("TopCoins")]
    public async Task<IActionResult> GetTopCoins()
    {
        var coins = await _coinloreService.FetchCoinsInBatchAsync(0, 100);
        return Ok(coins);
    }
}