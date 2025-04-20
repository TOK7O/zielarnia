using Zielarnia.Core.Models.Base;

namespace Zielarnia.Data.Models;
public class Supplier : EntityBase
{
    public string Name { get; set; } = string.Empty;
    public string? ContactInfo { get; set; }
    public int? AddressId { get; set; }
}