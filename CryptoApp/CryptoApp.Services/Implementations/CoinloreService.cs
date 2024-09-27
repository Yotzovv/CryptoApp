using CryptoApp.Data.dtos;
using CryptoApp.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using System.Net.Http;

namespace CryptoApp.Services.Implementations;

public class CoinloreService : ICoinloreService
{
    private readonly IConfiguration _configuration;
    private readonly HttpClient _httpClient;

    public static Dictionary<string, int> CoinInfoCache { get; } = new();

    public CoinloreService(IConfiguration configuration, IHttpClientFactory httpClientFactory)
    {
        _configuration = configuration;
        _httpClient = httpClientFactory.CreateClient();
    }

    public async Task<List<CoinloreItemDto>> FetchCoinsInBatchAsync(int start, int limit)
    {
        try
        {
            var url = $"{_configuration["CoinloreURL"]}/tickers/?start={start}&limit={limit}";
            var response = await _httpClient.GetStringAsync(url);

            var dataToken = JObject.Parse(response)["data"];
            var coinInfoData = dataToken?.ToObject<List<CoinloreItemDto>>() ?? new List<CoinloreItemDto>();

            foreach (var coin in coinInfoData)
            {
                if (!CoinInfoCache.ContainsKey(coin.Symbol))
                {
                    CoinInfoCache.Add(coin.Symbol, coin.Id);
                }
            }

            return coinInfoData;
        }
        catch (HttpRequestException)
        {
            throw new Exception("An error occurred while fetching coins from Coinlore API. Please try again later.");
        }
    }
}