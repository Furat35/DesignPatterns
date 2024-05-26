using MassTransit;
using Shared;
using Shared.Events;

namespace Payment.API.Consumers
{
    public class StockReservedRequestPaymentConsumer : IConsumer<StockReservedRequestPayment>
    {
        private readonly ILogger<StockReservedRequestPaymentConsumer> _logger;
        private readonly IPublishEndpoint _publishEndpoint;

        public StockReservedRequestPaymentConsumer(IPublishEndpoint publishEndpoint, ILogger<StockReservedRequestPaymentConsumer> logger)
        {
            _publishEndpoint = publishEndpoint;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<StockReservedRequestPayment> context)
        {
            var userBalance = 3000m;
            if (userBalance >= context.Message.Payment.TotalPrice)
            {
                _logger.LogInformation($"${context.Message.Payment.TotalPrice} TL is withdrawn from user id : {context.Message.BuyerId}");
                await _publishEndpoint.Publish(new PaymentCompletedEvent(context.Message.CorrelationId));
            }
            else
            {
                _logger.LogInformation($"Balance is not enough for user id : {context.Message.BuyerId}");
                await _publishEndpoint.Publish(new PaymentFailedEvent(context.Message.CorrelationId)
                {
                    Reason = "Not enough balance!",
                    OrderItems = context.Message.OrderItems
                });
            }
        }
    }
}
