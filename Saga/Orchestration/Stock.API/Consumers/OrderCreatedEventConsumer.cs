using MassTransit;
using Microsoft.EntityFrameworkCore;
using Shared;
using Shared.Contracts;
using Shared.Events;
using Stock.API.Models;

namespace Stock.API.Consumers
{
    public class OrderCreatedEventConsumer : IConsumer<IOrderCreatedEvent>
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

        public async Task Consume(ConsumeContext<IOrderCreatedEvent> context)
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

                _logger.LogInformation($"Stock is reserved for Correlation Id : {context.Message.CorrelationId}");
                var stockReservedEvent = new StockReservedEvent(context.Message.CorrelationId) { OrderItems = context.Message.OrderItems };

                await _publishEndpoint.Publish(stockReservedEvent);
            }
            else
            {
                await _publishEndpoint.Publish(new StockNotReservedEvent(context.Message.CorrelationId)
                {
                    Reason = "Not enough stock!"
                });
                _logger.LogInformation($"Stock is not reserved for Correlation Id : {context.Message.CorrelationId}");
            }
        }
    }
}
