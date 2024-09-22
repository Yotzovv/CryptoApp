namespace CryptoApp.Data.dtos;

public class CurrencyDto
{
    public Guid Id { get; set; }
    public string Coin { get; set; }
    public double Amount { get; set; }
    public decimal InitialBuyPrice { get; set; }
    public decimal CurrentPrice { get; set; }
    public decimal InitialValue { get; set; }
    public decimal CurrentValue { get; set; }
    
    public double ChangePercentage { get; set; }
}