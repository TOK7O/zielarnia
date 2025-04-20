using Microsoft.Extensions.Configuration;
using Zielarnia.Core.Models;
using Zielarnia.Core.Services;
using Zielarnia.Data.Services;
using Zielarnia.UI;
using Zielarnia.Core.Interfaces;
using Zielarnia.Core.Events;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Zielarnia;

internal class Program
{
    private static ILoggerService? _logger;

    static async Task Main(string[] args)
    {
        IConfiguration configuration;
        try
        {
            configuration = new ConfigurationBuilder()
               .SetBasePath(Directory.GetCurrentDirectory())
               .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
               .Build();

            if (string.IsNullOrEmpty(configuration.GetConnectionString("DefaultConnection")))
            {
                throw new InvalidOperationException("Connection string 'DefaultConnection' not found in appsettings.json.");
            }
            if (string.IsNullOrEmpty(configuration["UserSettings:UsersFilePath"]))
            {
                throw new InvalidOperationException("User settings 'UsersFilePath' not found in appsettings.json.");
            }
        }
        catch (Exception ex)
        {
            ConsoleHelper.WriteError($"Krytyczny błąd konfiguracji: {ex.Message}");
            ConsoleHelper.WriteError("Sprawdź, czy plik 'appsettings.json' istnieje i zawiera wymagane klucze ('ConnectionStrings:DefaultConnection', 'UserSettings:UsersFilePath').");
            Console.WriteLine("\nNaciśnij dowolny klawisz aby zakończyć...");
            Console.ReadKey();
            return;
        }


        try
        {
            _logger = new FileLoggerService(configuration);
            _logger.LogSaved += Logger_LogSaved;

            var dbService = new DatabaseService(configuration);
            await dbService.TestConnectionAsync();

            var authService = new AuthenticationService(configuration, _logger);
            var menuService = new MenuService(dbService, _logger);

            authService.LoginAttempt += AuthService_LoginAttempt;
            authService.UserLoggedIn += AuthService_UserLoggedIn;


            await _logger.LogInfoAsync("Application started successfully.");
            Console.WriteLine("Witaj w aplikacji Zielarnia!");

            User? currentUser = null;
            while (currentUser == null)
            {
                Console.WriteLine("\n--- Logowanie ---");
                string login = ConsoleHelper.ReadString("Podaj login:");
                string password = ConsoleHelper.ReadPassword("Podaj hasło:");

                try
                {
                    currentUser = await authService.LoginAsync(login, password);

                    if (currentUser == null)
                    {
                        ConsoleHelper.WriteError("Nieprawidłowy login lub hasło. Spróbuj ponownie.");
                        await Task.Delay(1500);
                        Console.Clear();
                    }
                }
                catch (Exception ex) // Łapie nieoczekiwane błędy podczas samego procesu logowania.
                {
                    ConsoleHelper.WriteError($"Wystąpił nieoczekiwany błąd podczas próby logowania. Sprawdź logi.");
                    await _logger.LogErrorAsync($"Unexpected error during LoginAsync call for user '{login}' in Program.cs", ex);
                    await Task.Delay(2000);
                    Console.Clear();
                }
            }

            Console.Clear();
            ConsoleHelper.WriteSuccess($"Zalogowano pomyślnie jako: {currentUser.Login} (Rola: {currentUser.Role})");
            Console.WriteLine("\nWciśnij dowolny klawisz, aby przejść do menu głównego...");
            Console.ReadKey();


            await menuService.ShowMainMenu(currentUser);

        }
        catch (InvalidOperationException dbEx) when (dbEx.Message.Contains("Failed to connect"))
        {
            ConsoleHelper.WriteError($"Nie można połączyć się z bazą danych. Sprawdź konfigurację i status serwera.");
            await (_logger?.LogErrorAsync("Application startup failed due to database connection error.", dbEx) ?? Task.CompletedTask);
        }
        catch (Exception ex)
        {
            ConsoleHelper.WriteError($"Wystąpił krytyczny błąd aplikacji: {ex.Message}. Sprawdź logi.");
            await (_logger?.LogErrorAsync("Critical application error occurred outside login/menu loop.", ex) ?? Task.CompletedTask);
        }
        finally
        {
            await (_logger?.LogInfoAsync("Application shutting down.") ?? Task.CompletedTask);
            Console.WriteLine("\nNaciśnij dowolny klawisz aby zamknąć okno...");
            Console.ReadKey();
        }
    }
    private static Task AuthService_LoginAttempt(object sender, UserActionEventArgs e)
    {
        return _logger?.LogInfoAsync($"Login attempt recorded for user: '{e.Login}'. Timestamp: {e.Timestamp:O}") ?? Task.CompletedTask;
    }
    private static Task AuthService_UserLoggedIn(object sender, UserActionEventArgs e)
    {
        if (e.Success && _logger != null)
        {
            return _logger.LogInfoAsync($"Event Handler: User '{e.Login}' login confirmed. Role assigned. Details: {e.Message}");
        }
        return Task.CompletedTask;
    }
    private static Task Logger_LogSaved(string logEntry)
    {
        return Task.CompletedTask;
    }
}