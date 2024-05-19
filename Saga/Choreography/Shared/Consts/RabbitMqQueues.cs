namespace Shared.Consts
{
    public class RabbitMqQueues
    {
        public const string Stock_OrderCreated_EventQueueName = "stock-order-created-queue";
        public const string StockReserved_EventQueueName = "stock-reserved-queue";
        public const string StockPaymentFailed_EventQueueName = "stock-payment-failed-queue";
        //public const string Payment_StockReserved_EventQueueName = "payment-stock-reserved-queue";
        public const string OrderPaymentCompleted_EventQueueName = "order-payment-completed-queue";
        public const string OrderPaymentFailed_EventQueueName = "order-payment-failed-queue";
        public const string OrderStockNotReserved_EventQueueName = "order-stock-not-reserved-queue";
    }
}
