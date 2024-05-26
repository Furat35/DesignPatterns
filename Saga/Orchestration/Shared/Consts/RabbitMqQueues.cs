namespace Shared.Consts
{
    public class RabbitMqQueues
    {
        public const string OrderSaga = "order-sage-queue";

        public const string PaymentStockReservedRequestQueueName = "payment-stock-reserved-request-queue";
        public const string StockRollbackMessageQueueName = "stock-rollback-queue";
        public const string Stock_OrderCreated_EventQueueName = "stock-order-created-queue";
        public const string OrderRequestCompleted_EventQueueName = "order-request-completed-queue";
        public const string OrderRequestFailed_EventQueueName = "order-request-failed-queue";
    }
}
