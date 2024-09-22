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

        return portfolioDto;
    }

    public async Task Upload(IFormFile? file, AspNetUser user)
    {
        using var reader = new StreamReader(file!.OpenReadStream());

        var portfolio = await _portfolioRepository.FindAsync(x => x.Id == user.Id);

        while (await reader.ReadLineAsync() is { } line)
        {
            var currency = PortfolioHelpers.ProcessLine(line, user, portfolio);
            
            portfolio.Currencies.Add(currency);
        }

        await _context.SaveChangesAsync();
    }
}