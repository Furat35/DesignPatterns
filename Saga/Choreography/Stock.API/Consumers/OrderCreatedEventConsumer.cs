using MassTransit;
using MassTransit.Transports;
using Microsoft.EntityFrameworkCore;
using Shared.Consts;
using Shared.Events;
using Stock.API.Models;

namespace Stock.API.Consumers
{
    public class OrderCreatedEventConsumer : IConsumer<OrderCreatedEvent>
    {
        private readonly AppDbContext _appDbContext;
        private readonly IPublishEndpoint _publishEndpoint;
        private readonly ISendEndpointProvider _sendEndpoint;
        private readonly ILogger<OrderCreatedEventConsumer> _logger;

        public OrderCreatedEventConsumer(AppDbContext appDbContext, IPublishEndpoint publishEndpoint, ISendEndpointProvider sendEndpoint, 
            ILogger<OrderCreatedEventConsumer> logger)
        {
            _appDbContext = appDbContext;
            _publishEndpoint = publishEndpoint;
            _sendEndpoint = sendEndpoint;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<OrderCreatedEvent> context)
        {
            var stockResult = new List<bool>();
            foreach (var item in context.Message.OrderItems)
            {
                stockResult.Add(await _appDbContext.Stocks.AnyAsync(_ => _.ProductId == item.ProductId && _.Count >= item.Count));
            }

            if (stockResult.All(_ => _))
            {
                foreach (var item in context.Message.OrderItems)
                {
                    var stock = await _appDbContext.Stocks.FirstOrDefaultAsync(_ => _.ProductId == item.ProductId);
                    if (stock != null)
                        stock.Count -= item.Count;
                }

                await _appDbContext.SaveChangesAsync();

                _logger.LogInformation($"Stock is reserved for buyer id : {context.Message.BuyerId}");
                var sendEndpoint = await _sendEndpoint.GetSendEndpoint(new Uri($"queue:{RabbitMqQueues.StockReserved_EventQueueName}"));

                StockReservedEvent stockReservedEvent = new StockReservedEvent
                {
                    Payment = context.Message.Payment,
                    BuyerId = context.Message.BuyerId,
                    OrderId = context.Message.OrderId,
                    OrderItems = context.Message.OrderItems
                };
                await _sendEndpoint.Send(stockReservedEvent);
            }
            else
            {
                await _publishEndpoint.Publish(new StockNotReservedEvent
                {
                    OrderId = context.Message.OrderId,
                    Message = "Not enough stock!"
                });
                _logger.LogInformation($"Stock is not reserved for buyer id : {context.Message.BuyerId}");
            }
        }
    }
}
