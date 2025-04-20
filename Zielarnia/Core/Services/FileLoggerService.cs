using Microsoft.Extensions.Configuration;
using Zielarnia.Core.Interfaces;
using System.Text;
using System.Threading;
using System.IO;

namespace Zielarnia.Core.Services;

public class FileLoggerService : ILoggerService
{
    private readonly string _logFilePath;
    // Obiekt u�ywany do synchronizacji dost�pu do pliku logu.
    private static readonly object _lock = new object();

    public event Func<string, Task>? LogSaved;

    public FileLoggerService(IConfiguration configuration)
    {
        _logFilePath = configuration["UserSettings:LogsFilePath"]
            ?? Path.Combine(Directory.GetCurrentDirectory(), "logs.txt");

        try
        {
            var logDirectory = Path.GetDirectoryName(_logFilePath);
            // Zapewnienie, �e katalog dla pliku log�w istnieje.
            if (!string.IsNullOrEmpty(logDirectory) && !Directory.Exists(logDirectory))
            {
                Directory.CreateDirectory(logDirectory);
            }
        }
        catch (Exception ex)
        {
            // Krytyczny b��d - zapis do konsoli, poniewa� logowanie do pliku mo�e by� niemo�liwe.
            Console.ForegroundColor = ConsoleColor.DarkRed;
            Console.WriteLine($"FATAL: Nie mo�na utworzy� katalogu log�w '{Path.GetDirectoryName(_logFilePath)}': {ex.Message}");
            Console.ResetColor();
        }
    }

    public async Task LogInfoAsync(string message)
    {
        await WriteLogAsync("INFO", message);
    }

    public async Task LogWarningAsync(string message)
    {
        await WriteLogAsync("WARN", message);
    }

    public async Task LogErrorAsync(string message, Exception? exception = null)
    {
        var fullMessage = new StringBuilder();
        fullMessage.Append(message);
        if (exception != null)
        {
            var currentEx = exception;
            int level = 0;
            // Dodaj szczeg�y zagnie�d�onych wyj�tk�w, aby u�atwi� diagnoz�.
            while (currentEx != null && level < 5)
            {
                fullMessage.AppendLine($" | L{level} Exception: {currentEx.GetType().Name}: {currentEx.Message}");
                fullMessage.Append($" | StackTrace: {currentEx.StackTrace}");
                currentEx = currentEx.InnerException;
                level++;
                if (currentEx != null) fullMessage.AppendLine(" ---> Inner Exception:");
            }
        }
        await WriteLogAsync("ERROR", fullMessage.ToString());
    }

    private async Task WriteLogAsync(string level, string message)
    {
        string logEntry = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff} [{level}] {message}{Environment.NewLine}";

        bool lockTaken = false;
        try
        {
            // Blokada zapewniaj�ca, �e tylko jeden w�tek na raz zapisuje do pliku.
            Monitor.Enter(_lock, ref lockTaken);
            using (var stream = new FileStream(_logFilePath, FileMode.Append, FileAccess.Write, FileShare.Read))
            using (var writer = new StreamWriter(stream, Encoding.UTF8))
            {
                await writer.WriteAsync(logEntry);
            }

            if (LogSaved != null)
            {
                _ = Task.Run(() => LogSaved.Invoke(logEntry));
            }
        }
        catch (Exception ex)
        {
            // B��d zapisu do pliku - zapisz informacje o b��dzie na konsoli.
            Console.ForegroundColor = ConsoleColor.DarkRed;
            Console.WriteLine($"FATAL: Nie mo�na zapisa� do pliku log�w '{_logFilePath}': {ex.Message}");
            Console.ResetColor();
        }
        finally
        {
            // Zawsze zwolnij blokad�, je�li zosta�a zdobyta.
            if (lockTaken)
            {
                Monitor.Exit(_lock);
            }
        }
    }
}