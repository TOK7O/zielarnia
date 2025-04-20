using Zielarnia.Core.Models.Base;

namespace Zielarnia.Data.Models;

public class Address : EntityBase
{
    public string Country { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string Street { get; set; } = string.Empty;
    public string BuildingNumber { get; set; } = string.Empty;
    public string? ApartmentNumber { get; set; }
}