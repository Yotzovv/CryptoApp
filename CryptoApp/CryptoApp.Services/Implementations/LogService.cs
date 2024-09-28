using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Text;
using CryptoApp.Services.Interfaces;

namespace CryptoApp.Services.Implementations;

public class LogService : ILogService
{
    private readonly ILogger<LogService> _logProvider;
    private readonly string _logFileLocation;

    public LogService(ILogger<LogService> logProvider)
    {
        _logProvider = logProvider;
        // Change the log file path to a directory within your project
        _logFileLocation = Path.Combine(Directory.GetCurrentDirectory(), "Logs", "application_log.txt");
        SetupLogFile();
    }

    public void LogInfo(string infoMessage)
    {
        _logProvider.LogInformation(infoMessage);
        SaveLogEntry(infoMessage);
    }

    public void LogError(string errorMessage, Exception exception = null)
    {
        string fullErrorMessage = exception == null ? errorMessage : $"{errorMessage} - {exception.Message}";
        _logProvider.LogError(fullErrorMessage);
        SaveLogEntry($"Error: {fullErrorMessage}");
    }

    private void SaveLogEntry(string entry)
    {
        var formattedEntry = $"{DateTime.Now:G}: {entry}{Environment.NewLine}";

        // Ensure the directory exists
        var logDir = Path.GetDirectoryName(_logFileLocation);
        if (!Directory.Exists(logDir))
        {
            Directory.CreateDirectory(logDir);
        }

        // Append the log entry to the file
        File.AppendAllText(_logFileLocation, formattedEntry);
    }

    private void SetupLogFile()
    {
        var logDir = Path.GetDirectoryName(_logFileLocation);
        if (!Directory.Exists(logDir))
        {
            Directory.CreateDirectory(logDir);
        }

        if (!File.Exists(_logFileLocation))
        {
            var initialContent = $"Log File Initialized at {DateTime.Now:G}{Environment.NewLine}";
            File.WriteAllText(_logFileLocation, initialContent);
        }
    }
}