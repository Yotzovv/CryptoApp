namespace CryptoApp.Data.dtos;

public class PortfolioDto
{
    public Guid Id { get; set; }
    
    public double InitialPortfolioValue { get; set; }
    
    public double CurrentPortfolioValue { get; set; }
    
    public double OverallChangePercentage { get; set; }
    
    public List<CurrencyDto> Currencies { get; set; } = [];
    
    public required AspNetUserDto User { get; set; }
}