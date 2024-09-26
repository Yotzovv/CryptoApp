using System.Globalization;
using CryptoApp.Data.dtos;
using CryptoApp.Data.Models;

namespace CryptoApp.Services.Helpers;

public static class PortfolioHelpers
{
    public static Currency ProcessLine(string line, AspNetUser user, Portfolio portfolio)
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
            InitialBuyPrice = initialBuyPrice,
            PortfolioId = portfolio!.Id,
            Portfolio = portfolio
        };

        return currency;
    }
    
    public static PortfolioDto MapToPortfolioDto(Portfolio portfolio)
    {
        var portfolioDto = new PortfolioDto
        {
            Id = portfolio.Id,
            User = new AspNetUserDto() { Id = portfolio.User.Id, Email = portfolio.User.Email },
            Currencies = portfolio.Currencies.Select(x => new CurrencyDto
            {
                Id = x.Id,
                Amount = x.Amount,
                Coin = x.Coin,
                InitialBuyPrice = x.InitialBuyPrice,
                CurrentPrice = x.CurrentPrice,
            }).ToList()
        };
        return portfolioDto;
    }
}