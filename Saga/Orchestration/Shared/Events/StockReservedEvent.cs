using Shared.Contracts;
using Shared.Messages;

namespace Shared
{
    public class StockReservedEvent : IStockReservedEvent
    {
        public StockReservedEvent(Guid correlationId)
        {
            CorrelationId = correlationId;
        }
        public List<OrderItemMessage> OrderItems { get; set; } = new List<OrderItemMessage>();
        public Guid CorrelationId { get; }
    }
}
