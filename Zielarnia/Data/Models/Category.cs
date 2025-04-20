using Zielarnia.Core.Models.Base;

namespace Zielarnia.Data.Models;

public class Category : EntityBase
{
    public string Name { get; set; } = string.Empty;
}