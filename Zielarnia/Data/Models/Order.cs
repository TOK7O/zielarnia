using Zielarnia.Core.Models.Base;
using Zielarnia.Core.Models.Enums;

namespace Zielarnia.Data.Models;

public class Order : EntityBase
{
    public int? ClientId { get; set; }
    public DateTime OrderDate { get; set; } = DateTime.UtcNow;
    public OrderStatus Status { get; set; }
}