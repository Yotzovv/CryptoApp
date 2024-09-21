using Microsoft.AspNetCore.Http;

namespace CryptoApp.Services.Interfaces;

public interface IPortfolioService
{
    Task Upload(IFormFile? file);
    
    void Get();
}