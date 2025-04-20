namespace Zielarnia.Core.Events;

public class UserActionEventArgs : EventArgs
{
    public string Login { get; }
    public DateTime Timestamp { get; }
    public bool Success { get; }
    public string? Message { get; }

    public UserActionEventArgs(string login, bool success, string? message = null)
    {
        Login = login;
        Timestamp = DateTime.UtcNow; // Używa UTC dla spójności
        Success = success;
        Message = message;
    }
}