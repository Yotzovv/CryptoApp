using CryptoApp.Data;
using CryptoApp.Data.Models;
using CryptoApp.Services.Interfaces;
using Microsoft.AspNetCore.Http;

namespace CryptoApp.Services.Implementations;

public class PortfolioService : IPortfolioService
{
    private readonly CryptoAppDbContext _context;

    public PortfolioService(CryptoAppDbContext context)
    {
        _context = context;
    }

    public void Get()
    {
        throw new NotImplementedException();
    }
    
    public async Task Upload(IFormFile? file)
    {
        using var reader = new StreamReader(file!.OpenReadStream());
        var fileContent = await reader.ReadToEndAsync();
        
        var lines = fileContent.Split("\n");
        
        foreach (var line in lines)
        {
            var currency = await ProcessLine(line);
        
            _context.Currencies.Add(currency);

            await _context.SaveChangesAsync();
        }
    }
    
    private async Task<Currency> ProcessLine(string line)
    {
        var parts = line.Split("|");
        
        if (parts.Length != 3)
            throw new Exception("Invalid file format.");
        
        var coinsOwned = double.Parse(parts[0]);
        var coinName = parts[1];
        var initialBuyPrice = decimal.Parse(parts[2]);
        
        var currency = new Currency
        {
            Amount = coinsOwned,
            Coin = coinName,
            InitialBuyPrice = initialBuyPrice
        };

        return currency;
    }
}