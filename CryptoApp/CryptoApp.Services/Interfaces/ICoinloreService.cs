using CryptoApp.Data.dtos;

namespace CryptoApp.Services.Interfaces;

public interface ICoinloreService
{
    Task<List<CoinloreItemDto>> FetchCoinsInBatchAsync(int start, int limit);
}