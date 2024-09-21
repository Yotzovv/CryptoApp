namespace CryptoApp.Data.Models;

public class Portfolio
{
    public double InitialPortfolioValue { get; set; }
    public double CurrentPortfolioValue { get; set; }
    public double OverallChangePercentage { get; set; }
    public List<Currency> PortfolioDetails { get; set; } = [];
}