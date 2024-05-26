using MassTransit;
using Shared.Messages;

namespace Shared.Contracts
{
    public interface IStockReservedEvent : CorrelatedBy<Guid>
    {
        List<OrderItemMessage> OrderItems { get; set; }
    }
}
