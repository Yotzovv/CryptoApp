using System.Globalization;
using CryptoApp.Data;
using CryptoApp.Data.dtos;
using CryptoApp.Data.Models;
using CryptoApp.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace CryptoApp.Services.Implementations;

public class PortfolioService : IPortfolioService
{
    private readonly CryptoAppDbContext _context;

    public PortfolioService(CryptoAppDbContext context)
    {
        _context = context;
    }

    public async Task<PortfolioDto> Get(Guid userId)
    {
        var portfolio = await _context.Portfolios
            .Include(x => x.Currencies)
            .Include(x => x.User)
            .SingleAsync(x => x.UserId == userId);
        
        var portfolioDto = new PortfolioDto
        {
            Id = portfolio.Id,
            User = new AspNetUserDto() { Id = portfolio.User.Id, Email = portfolio.User.Email },
            Currencies = portfolio.Currencies.Select(x => new CurrencyDto
            {
                Id = x.Id,
                Amount = x.Amount,
                Coin = x.Coin,
                InitialBuyPrice = x.InitialBuyPrice
            }).ToList()
        };

        return portfolioDto;
    }

    public async Task Upload(IFormFile? file, AspNetUser user)
    {
        using var reader = new StreamReader(file!.OpenReadStream());

        while (await reader.ReadLineAsync() is { } line)
        {
            var currency = ProcessLine(line, user);
            _context.Currencies.Add(currency);
        }

        await _context.SaveChangesAsync();
    }

    private Currency ProcessLine(string line, AspNetUser user)
    {
        var parts = line.Split("|");

        if (parts.Length != 3)
            throw new Exception("Invalid file format.");

        if (!double.TryParse(parts[0], NumberStyles.Number, CultureInfo.InvariantCulture, out var coinsOwned))
            throw new Exception("Invalid amount value.");

        var coinName = parts[1];

        if (!decimal.TryParse(parts[2], NumberStyles.Number, CultureInfo.InvariantCulture, out var initialBuyPrice))
            throw new Exception("Invalid price value.");
        
        var portfolio = _context.Portfolios.FirstOrDefault(x => x.UserId == user.Id);

        var currency = new Currency
        {
            Amount = coinsOwned,
            Coin = coinName,
            InitialBuyPrice = initialBuyPrice,
            PortfolioId = portfolio!.Id,
            Portfolio = portfolio
        };

        return currency;
    }
}