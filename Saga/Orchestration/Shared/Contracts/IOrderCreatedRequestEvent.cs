using Shared.Messages;

namespace Shared.Contracts
{
    public interface IOrderCreatedRequestEvent
    {
        int OrderId { get; set; }
        string BuyerId { get; set; }
        PaymentMessage Payment { get; set; }
        List<OrderItemMessage> OrderItems { get; set; }
    }
}
