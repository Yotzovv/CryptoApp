namespace CryptoApp.Data.Models;

public class Portfolio
{
    public Guid Id { get; set; }
    
    public double InitialPortfolioValue { get; set; }
    
    public double CurrentPortfolioValue { get; set; }
    
    public double OverallChangePercentage { get; set; }
    
    public List<Currency> Currencies { get; set; } = [];
    
    public Guid UserId { get; set; }
    
    public required AspNetUser User { get; set; }
}