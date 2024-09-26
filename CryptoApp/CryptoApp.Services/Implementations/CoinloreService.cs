using CryptoApp.Data.dtos;
using CryptoApp.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;

namespace CryptoApp.Services.Implementations;

public class CoinloreService : ICoinloreService
{
    private readonly string _coinloreBaseUrl;
    private static readonly HttpClient _httpClient = new HttpClient();
    public static Dictionary<string, int> CoinInfoCache { get; } = new Dictionary<string, int>();
    private readonly IConfiguration _configuration;

    public CoinloreService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task<List<CoinloreItemDto>> FetchCoinsInBatchAsync(int start, int limit)
    {
        try
        {
            var url = $"{_configuration["CoinloreURL"]}/tickers/?start={start}&limit={limit}";
            var response = await _httpClient.GetStringAsync(url);

            var dataToken = JObject.Parse(response)["data"];
            var coinInfoData = dataToken?.ToObject<List<CoinloreItemDto>>() ?? [];

            foreach (var coin in coinInfoData)
            {
                if (!CoinInfoCache.ContainsKey(coin.Symbol))
                {
                    CoinInfoCache.Add(coin.Symbol, coin.Id);
                }
            }

            return coinInfoData;
        }
        catch (HttpRequestException httpEx)
        {
            // _loggingService.LogError("Error occurred while fetching coins from Coinlore API", httpEx);
            throw new Exception("An error occurred while fetching coins from Coinlore API. Please try again later.");
        }
    }
    
    public int? GetCoinIdBySymbolAsync(string symbol)
    {
        if (CoinInfoCache.TryGetValue(symbol, out var id))
        {
            return id;
        }

        return null;
    }
    
    public async Task<CoinloreItemDto[]?> GetCoinsDetailsByIdsAsync(string coinIds)
    {
        try
        {
            var response = await _httpClient.GetStringAsync($"{_coinloreBaseUrl}/ticker/?id={coinIds}");

            var coinArray = JArray.Parse(response);

            var coinData = coinArray.ToObject<CoinloreItemDto[]>();

            return coinData;
        }
        catch (HttpRequestException httpEx)
        {
            throw new Exception("An error occurred while fetching coin details from the Coinlore API. Please try again later.");
        }
    }
    
}