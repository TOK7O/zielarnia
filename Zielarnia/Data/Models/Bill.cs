using Zielarnia.Core.Models.Base;

namespace Zielarnia.Data.Models;

public class Bill : EntityBase
{
    public DateOnly BillDate { get; set; }
    public int BillTypeId { get; set; }
    public decimal Amount { get; set; }
}