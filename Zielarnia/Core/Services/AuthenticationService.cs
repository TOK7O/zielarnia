using BCryptNet = BCrypt.Net.BCrypt;
using Microsoft.Extensions.Configuration;
using Zielarnia.Core.Models;
using Zielarnia.Core.Models.Enums;
using Zielarnia.UI;
using Zielarnia.Core.Interfaces;
using Zielarnia.Core.Events;
using System.IO;
using System;
using System.Threading.Tasks;
using System.Linq;

namespace Zielarnia.Core.Services;

// Delegat do obsługi zdarzeń związanych z akcjami użytkownika.
public delegate Task UserActionEventHandler(object sender, UserActionEventArgs e);

// Klasa odpowiadająca za uwierzytelnianie użytkowników na podstawie pliku.
public class AuthenticationService
{
    private readonly string _usersFilePath;
    private readonly ILoggerService _logger;

    // Zdarzenie wywoływane po pomyślnym zalogowaniu użytkownika.
    public event UserActionEventHandler? UserLoggedIn;

    // Zdarzenie wywoływane podczas próby logowania
    public event UserActionEventHandler? LoginAttempt;

    public AuthenticationService(IConfiguration configuration, ILoggerService logger)
    {
        _logger = logger;
        // Pobiera ścieżkę pliku użytkowników z konfiguracji lub rzuca wyjątek, jeśli nie została znaleziona.
        _usersFilePath = configuration["UserSettings:UsersFilePath"]
            ?? throw new InvalidOperationException("User settings 'UsersFilePath' not found in configuration.");
    }

    // Próbuje zalogować użytkownika na podstawie danych z pliku. 
    public async Task<User?> LoginAsync(string login, string password)
    {
        string? failureReason = null;

        await OnLoginAttempt(new UserActionEventArgs(login, false));

        if (!File.Exists(_usersFilePath))
        {
            failureReason = $"Users file not found at '{_usersFilePath}'";
            ConsoleHelper.WriteError($"Błąd: {failureReason}");
            await _logger.LogErrorAsync($"Login failed for user '{login}'. Reason: {failureReason}");
            return null;
        }

        try
        {
            string[] lines = await File.ReadAllLinesAsync(_usersFilePath);
            foreach (var line in lines)
            {
                if (string.IsNullOrWhiteSpace(line) || line.TrimStart().StartsWith("#")) continue;

                var parts = line.Split(',');
                if (parts.Length != 3)
                {
                    ConsoleHelper.WriteWarning($"Ostrzeżenie: Pomijanie nieprawidłowej linii w pliku użytkowników: {line}");
                    await _logger.LogWarningAsync($"Invalid line format in users file, skipping line: {line}");
                    continue;
                }

                var storedLogin = parts[0].Trim();
                var storedHash = parts[1].Trim();
                var roleString = parts[2].Trim();

                if (storedLogin.Equals(login, StringComparison.OrdinalIgnoreCase))
                {
                    bool passwordVerified = false;
                    try
                    {
                        passwordVerified = BCryptNet.Verify(password, storedHash);
                    }
                    catch (BCrypt.Net.SaltParseException ex) // Obsługa specyficznego błędu BCrypt dotyczącego formatu hasha
                    {
                        failureReason = "Invalid hash format in users file.";
                        ConsoleHelper.WriteError($"Błąd: {failureReason} dla użytkownika '{login}'");
                        await _logger.LogErrorAsync($"Login failed for user '{login}'. Reason: {failureReason}", ex);
                        return null;
                    }
                    catch (Exception ex) // Obsługa innych błędów podczas weryfikacji hasła.
                    {
                        failureReason = "Unexpected error during password verification.";
                        ConsoleHelper.WriteError($"Błąd: {failureReason}");
                        await _logger.LogErrorAsync($"Login failed for user '{login}'. Reason: {failureReason}", ex);
                        return null;
                    }


                    if (passwordVerified)
                    {
                        // Próba sparsowania roli użytkownika (ignorując wielkość liter).
                        if (Enum.TryParse<UserRole>(roleString, true, out var role))
                        {
                            var user = new User(storedLogin, role);
                            await OnUserLoggedIn(new UserActionEventArgs(login, true, $"Logged in as {role}"));
                            await _logger.LogInfoAsync($"User '{login}' logged in successfully as {role}.");
                            return user;
                        }
                        else
                        {
                            failureReason = $"Invalid role '{roleString}' for user '{login}' in users file.";
                            ConsoleHelper.WriteWarning($"Ostrzeżenie: {failureReason}");
                            await _logger.LogWarningAsync($"Login attempt failed for '{login}'. Reason: {failureReason}");
                            return null;
                        }
                    }
                    else
                    {
                        failureReason = "Incorrect password.";
                        await _logger.LogWarningAsync($"Login attempt failed for '{login}'. Reason: {failureReason}");
                        return null;
                    }
                }
            }
        }
        catch (IOException ex) // Obsługa błędów wejścia/wyjścia podczas czytania pliku.
        {
            failureReason = "Error reading users file.";
            ConsoleHelper.WriteError($"Błąd: {failureReason}");
            await _logger.LogErrorAsync($"Login failed for user '{login}'. Reason: {failureReason}", ex);
            return null;
        }
        catch (Exception ex) // Obsługa wszelkich innych nieoczekiwanych błędów.
        {
            failureReason = "An unexpected error occurred during login process.";
            ConsoleHelper.WriteError($"Błąd: {failureReason}");
            await _logger.LogErrorAsync($"Login failed for user '{login}'. Reason: {failureReason}", ex);
            return null;
        }

        failureReason = "User not found.";
        await _logger.LogWarningAsync($"Login attempt failed for '{login}'. Reason: {failureReason}");
        return null;
    }

    // Metoda pomocnicza do bezpiecznego wywoływania zdarzenia UserLoggedIn.
    protected virtual async Task OnUserLoggedIn(UserActionEventArgs e)
    {
        if (UserLoggedIn != null)
        {
            // Pobiera listę wszystkich subskrybentów (delegatów) zdarzenia.
            // Rzutuje je na właściwy typ delegata i tworzy tablicę zadań (Task) przez wywołanie każdego z nich.
            var handlers = UserLoggedIn.GetInvocationList()
                               .Cast<UserActionEventHandler>()
                               .Select(handler => handler(this, e))
                               .ToArray();
            try
            {
                // Oczekuje na zakończenie wszystkich zadań równolegle.
                await Task.WhenAll(handlers);
            }
            catch (Exception ex) // Loguje błąd, jeśli którykolwiek z subskrybentów rzucił wyjątek.
            {
                await _logger.LogErrorAsync($"Error occurred in one or more UserLoggedIn event subscribers: {ex.Message}", ex);
            }
        }
    }

    // Metoda pomocnicza do bezpiecznego wywoływania zdarzenia LoginAttempt.
    protected virtual async Task OnLoginAttempt(UserActionEventArgs e)
    {
        if (LoginAttempt != null)
        {
            // Analogicznie do OnUserLoggedIn, pobiera subskrybentów i tworzy zadania.
            var handlers = LoginAttempt.GetInvocationList()
                                .Cast<UserActionEventHandler>()
                                .Select(handler => handler(this, e))
                                .ToArray();
            try
            {
                // Oczekuje na zakończenie wszystkich zadań.
                await Task.WhenAll(handlers);
            }
            catch (Exception ex) // Loguje błąd, jeśli którykolwiek z subskrybentów rzucił wyjątek.
            {
                await _logger.LogErrorAsync($"Error occurred in one or more LoginAttempt event subscribers: {ex.Message}", ex);
            }
        }
    }
}