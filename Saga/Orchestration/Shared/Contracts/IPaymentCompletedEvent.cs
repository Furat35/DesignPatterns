using MassTransit;

namespace Shared.Contracts
{
    public interface IPaymentCompletedEvent : CorrelatedBy<Guid>
    {
    }
}
