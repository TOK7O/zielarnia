using Zielarnia.Core.Models.Base;

namespace Zielarnia.Data.Models;

public class Client : EntityBase
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public int AddressId { get; set; }
    public string? ParcelLockerId { get; set; }
}