using System.Globalization;
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

        string line;
        while ((line = await reader.ReadLineAsync()) != null)
        {
            try
            {
                var currency = ProcessLine(line);
                _context.Currencies.Add(currency);
            }
            catch (Exception ex)
            {
                // Optionally log or handle the exception for invalid lines
                // For now, we'll skip invalid lines
                continue;
            }
        }

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
        // Move SaveChangesAsync outside of the loop and avoid wrapping it in a using statement
    }

    private Currency ProcessLine(string line)
    {
        var parts = line.Split("|");

        if (parts.Length != 3)
            throw new Exception("Invalid file format.");

        if (!double.TryParse(parts[0], NumberStyles.Number, CultureInfo.InvariantCulture, out var coinsOwned))
            throw new Exception("Invalid amount value.");

        var coinName = parts[1];

        if (!decimal.TryParse(parts[2], NumberStyles.Number, CultureInfo.InvariantCulture, out var initialBuyPrice))
            throw new Exception("Invalid price value.");

        var currency = new Currency
        {
            Amount = coinsOwned,
            Coin = coinName,
            InitialBuyPrice = initialBuyPrice
        };

        return currency;
    }
}