using MassTransit;
using Shared.Events;

namespace Payment.API.Consumers
{
    public class StockReservedEventConsumer : IConsumer<StockReservedEvent>
    {
        private readonly ILogger<StockReservedEventConsumer> _logger;
        private readonly IPublishEndpoint _publishEndpoint;

        public StockReservedEventConsumer(IPublishEndpoint publishEndpoint, ILogger<StockReservedEventConsumer> logger)
        {
            _publishEndpoint = publishEndpoint;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<StockReservedEvent> context)
        {
            var userBalance = 3000m;
            if (userBalance >= context.Message.Payment.TotalPrice)
            {
                _logger.LogInformation($"${context.Message.Payment.TotalPrice} TL is withdrawn from user id : {context.Message.BuyerId}");
                await _publishEndpoint.Publish(new PaymentCompletedEvent { BuyerId = context.Message.BuyerId, OrderId = context.Message.OrderId });
            }
            else
            {
                _logger.LogInformation($"Balance is not enough for user id : {context.Message.BuyerId}");
                await _publishEndpoint.Publish(new PaymentFailedEvent { BuyerId = context.Message.BuyerId, OrderId = context.Message.OrderId, 
                    Message = "Not enough balance!", OrderItems = context.Message.OrderItems});
            }
        }
    }
}
