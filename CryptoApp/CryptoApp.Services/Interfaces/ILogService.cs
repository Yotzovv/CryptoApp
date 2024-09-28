namespace CryptoApp.Services.Interfaces;

public interface ILogService
{
    void LogInfo(string infoMessage);
    
    void LogError(string errorMessage, Exception exception = null);
    
    
}