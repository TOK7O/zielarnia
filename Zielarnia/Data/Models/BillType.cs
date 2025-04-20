using Zielarnia.Core.Models.Base;

namespace Zielarnia.Data.Models;
public class BillType : EntityBase
{
    public string TypeName { get; set; } = string.Empty;
    public string? Description { get; set; }
}