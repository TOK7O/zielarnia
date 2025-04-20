using System;
using System.Text;
using System.Globalization;
using System.IO;

namespace Zielarnia.UI;

// Klasa przechowuje metody pomagające z wyświetlaniem menu i komunikatów
public static class ConsoleHelper
{
    public static void ClearScreen()
    {
        try
        {
            for (int i = 0; i < 80; i++)
            {
                Console.WriteLine();
            }
        }
        catch (IOException) { }

        try
        {
            Console.Clear();
        }
        catch (IOException) { }
    }

    public static void DisplayHeader(string title)
    {
        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine($"--- {title.ToUpperInvariant()} ---");
        Console.ResetColor();
        Console.WriteLine();
    }

    public static void WriteMessage(string message)
    {
        Console.WriteLine(message);
    }

    public static void WriteSuccess(string message)
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"[OK] {message}");
        Console.ResetColor();
    }

    public static void WriteWarning(string message)
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine($"[UWAGA] {message}");
        Console.ResetColor();
    }

    public static void WriteError(string message)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"[BŁĄD] {message}");
        Console.ResetColor();
    }

    public static string ReadString(string prompt, bool allowEmpty = false)
    {
        string? input;
        do
        {
            Console.Write($"{prompt} ");
            input = Console.ReadLine();
            if (!allowEmpty && string.IsNullOrWhiteSpace(input))
            {
                WriteWarning("Wartość nie może być pusta. Spróbuj ponownie.");
            }
        } while (!allowEmpty && string.IsNullOrWhiteSpace(input));
        return input ?? string.Empty;
    }

    public static string ReadPassword(string prompt)
    {
        Console.Write($"{prompt} ");
        var password = new StringBuilder();
        while (true)
        {
            ConsoleKeyInfo keyInfo = Console.ReadKey(true);
            if (keyInfo.Key == ConsoleKey.Enter) { Console.WriteLine(); break; }
            if (keyInfo.Key == ConsoleKey.Backspace && password.Length > 0) { password.Remove(password.Length - 1, 1); Console.Write("\b \b"); }
            else if (!char.IsControl(keyInfo.KeyChar)) { password.Append(keyInfo.KeyChar); Console.Write("*"); }
        }
        return password.ToString();
    }

    public static int ReadInt(string prompt, int? minValue = null, int? maxValue = null)
    {
        int value;
        bool isValid;
        string? input;
        do
        {
            Console.Write($"{prompt} ");
            input = Console.ReadLine();
            isValid = int.TryParse(input, out value);
            if (!isValid) { WriteWarning("Nieprawidłowa liczba całkowita."); }
            else if (minValue.HasValue && value < minValue.Value) { WriteWarning($"Wartość musi być >= {minValue.Value}."); isValid = false; }
            else if (maxValue.HasValue && value > maxValue.Value) { WriteWarning($"Wartość musi być <= {maxValue.Value}."); isValid = false; }
        } while (!isValid);
        return value;
    }

    public static decimal ReadDecimal(string prompt, decimal? minValue = null)
    {
        decimal value;
        bool isValid;
        string? input;
        do
        {
            Console.Write($"{prompt} ");
            input = Console.ReadLine()?.Replace(',', '.');
            isValid = decimal.TryParse(input, NumberStyles.Any, CultureInfo.InvariantCulture, out value);
            if (!isValid) { WriteWarning("Nieprawidłowa liczba dziesiętna."); }
            else if (minValue.HasValue && value < minValue.Value) { WriteWarning($"Wartość musi być >= {minValue.Value:F2}."); isValid = false; }
        } while (!isValid);
        return value;
    }

    public static bool Confirm(string prompt)
    {
        string? input;
        do
        {
            Console.Write($"{prompt} (T/N): ");
            input = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(input) ||
               (!input.Trim().Equals("T", StringComparison.OrdinalIgnoreCase) &&
                !input.Trim().Equals("N", StringComparison.OrdinalIgnoreCase)))
            { WriteWarning("Wpisz 'T' (Tak) lub 'N' (Nie)."); input = null; }
        } while (input == null);
        return input.Trim().Equals("T", StringComparison.OrdinalIgnoreCase);
    }
}