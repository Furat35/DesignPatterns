using MassTransit;
using Shared.Messages;

namespace Shared.Contracts
{
    public interface IOrderCreatedEvent : CorrelatedBy<Guid>
    {
        List<OrderItemMessage> OrderItems { get; set; }
    }
}
