namespace Shared.Contracts
{
    public interface IOrderRequestCompletedEvent
    {
        public int OrderId { get; set; }
    }
}
