using System.Globalization;
using CryptoApp.Data;
using CryptoApp.Data.dtos;
using CryptoApp.Data.Models;
using CryptoApp.Data.Repository;
using CryptoApp.Services.Helpers;
using CryptoApp.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace CryptoApp.Services.Implementations;

public class PortfolioService : IPortfolioService
{
    private readonly CryptoAppDbContext _context;
    private readonly IRepository<Portfolio> _portfolioRepository;

    public PortfolioService(CryptoAppDbContext context, IRepository<Portfolio> portfolioRepository)
    {
        _context = context;
        _portfolioRepository = portfolioRepository;
    }

    public async Task<PortfolioDto> Get(Guid userId)
    {
        var portfolio = await _portfolioRepository.FindAsync(x => x.UserId == userId, 
            include: q => q
                .Include(c => c.Currencies)
                .Include(u => u.User));

        var portfolioDto = PortfolioHelpers.MapToPortfolioDto(portfolio);
        portfolioDto.CurrentPortfolioValue = portfolio.CurrentPortfolioValue;
        portfolioDto.InitialPortfolioValue = portfolio.InitialPortfolioValue;

        return portfolioDto;
    }
    
    public async Task Upload(IFormFile? file, AspNetUser user)
    {
        using var reader = new StreamReader(file!.OpenReadStream());

        var portfolio = _context.Portfolios.FirstOrDefault(x => x.UserId == user.Id);

        while (await reader.ReadLineAsync() is { } line)
        {
            var currency = PortfolioHelpers.ProcessLine(line, user, portfolio);

            if (!_context.Currencies.Where(x => x.PortfolioId == user.PortfolioId).Any(x => x.Coin == currency.Coin))
            {
                portfolio.Currencies.Add(currency);
            }
        }

        await _context.SaveChangesAsync();
    }
    
    public async Task UpdatePortfolio(AspNetUser user,  List<CoinloreItemDto> coinsInfoCache)
    {
        await CalculateCurrentPortfolioValue(user, coinsInfoCache);
        await CalculateInitialPortfolioValue(user);
        await UpdateOverallChangePercentage(user);
        await UpdateCoinsChangePercentages(user);
        
        await _context.SaveChangesAsync();
    }
    
    public async Task CalculateCurrentPortfolioValue(AspNetUser user, List<CoinloreItemDto> coinsInfoCache)
    {
        //TODO: use Repository pattern
       user.Portfolio = _context.Portfolios.Include(c => c.Currencies).FirstOrDefault(x => x.UserId == user.Id);
       var userCoins = user.Portfolio.Currencies;
        
        // Fetch current prices of all coins
        foreach (var coin in userCoins)
        {
            var coinInfo = coinsInfoCache.FirstOrDefault(x => x.Symbol == coin.Coin);
            coin.CurrentPrice = coinInfo?.Price_Usd ?? 0;
        }
        
        decimal portfolioValue = 0;
        
        userCoins.ForEach(x => portfolioValue += (decimal.Parse(x.Amount.ToString()) * x.CurrentPrice));
        
        user.Portfolio.CurrentPortfolioValue = (double)portfolioValue;
        
        _context.Portfolios.Update(user.Portfolio);
        _context.Currencies.UpdateRange(userCoins);
        
        await _context.SaveChangesAsync();
    }

    public async Task CalculateInitialPortfolioValue(AspNetUser user)
    {
        double initialPortfolioValue = 0;

        foreach (var coin in user.Portfolio.Currencies)
        {
            var sum = (double)coin.InitialBuyPrice * coin.Amount;
            initialPortfolioValue += sum;
        }

        user.Portfolio.InitialPortfolioValue = initialPortfolioValue;
        
        await _context.SaveChangesAsync();
    }

    public async Task UpdateOverallChangePercentage(AspNetUser user)
    {
        var portfolio = user.Portfolio;
        var currentPortfolioValue = portfolio.CurrentPortfolioValue;
        var initialPortfolioValue = portfolio.InitialPortfolioValue;
        
        var changePercentage = ((currentPortfolioValue - initialPortfolioValue) / initialPortfolioValue) * 100;
        
        portfolio.OverallChangePercentage = changePercentage;
        
        _context.Portfolios.Update(portfolio);

        await _context.SaveChangesAsync();
    }

    public async Task UpdateCoinsChangePercentages(AspNetUser user)
    {
        var portfolio = user.Portfolio;
        var coins = portfolio.Currencies;
        
        foreach (var coin in coins)
        {
            var changePercentage = ((coin.CurrentPrice - coin.InitialBuyPrice) / coin.InitialBuyPrice) * 100;
            
            coin.ChangePercentage = (double)changePercentage;
        }

        _context.Currencies.UpdateRange(coins);

        await _context.SaveChangesAsync();
    }
}