using Zielarnia.Core.Models.Base;

namespace Zielarnia.Data.Models;

public class Product : EntityBase
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public int? CategoryId { get; set; }
}