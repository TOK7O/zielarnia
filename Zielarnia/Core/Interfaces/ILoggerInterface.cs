namespace Zielarnia.Core.Interfaces;

public interface ILoggerService
{
    Task LogInfoAsync(string message);

    Task LogWarningAsync(string message);

    Task LogErrorAsync(string message, Exception? exception = null);

    // Definiuje zdarzenie oparte na delegacie Func<string, Task>, który przyjmuje string i zwraca Task.
    // Znak zapytania (?) oznacza, że zdarzenie może nie mieć subskrybentów (może być null).
    event Func<string, Task>? LogSaved;
}