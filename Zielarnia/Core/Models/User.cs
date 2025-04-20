using Zielarnia.Core.Models.Enums;

namespace Zielarnia.Core.Models;

public class User
{
    public string Login { get; private set; }
    public UserRole Role { get; private set; }

    public User(string login, UserRole role)
    {
        Login = login;
        Role = role;
    }
}