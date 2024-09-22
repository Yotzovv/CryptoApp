using CryptoApp.Data.Models;
using Microsoft.AspNetCore.Http;

namespace CryptoApp.Services.Interfaces;

public interface IPortfolioService
{
    Task Upload(IFormFile? file, AspNetUser? user);
    
    void Get();
}