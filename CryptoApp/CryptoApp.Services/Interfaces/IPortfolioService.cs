using CryptoApp.Data.dtos;
using CryptoApp.Data.Models;
using Microsoft.AspNetCore.Http;

namespace CryptoApp.Services.Interfaces;

public interface IPortfolioService
{
    Task Upload(IFormFile? file, AspNetUser user);
    
    Task<PortfolioDto> Get(Guid id);
    
    Task CalculateCurrentPortfolioValue(AspNetUser user, List<CoinloreItemDto> coinsInfoCache);

    Task CalculateInitialPortfolioValue(AspNetUser user);
}