using MassTransit;
using Microsoft.EntityFrameworkCore;
using Order.API.Models;
using Shared.Contracts;
using Shared.Events;

namespace Order.API.Consumers
{
    public class OrderRequestFailedEventConsumer : IConsumer<IOrderRequestFailedEvent>
    {
        private readonly AppDbContext _appDbContext;
        private readonly ILogger<OrderRequestFailedEventConsumer> _logger;

        public OrderRequestFailedEventConsumer(ILogger<OrderRequestFailedEventConsumer> logger, AppDbContext appDbContext)
        {
            _logger = logger;
            _appDbContext = appDbContext;
        }

        public async Task Consume(ConsumeContext<IOrderRequestFailedEvent> context)
        {
            var order = await _appDbContext.Orders.FirstOrDefaultAsync(_ => _.Id == context.Message.OrderId);
            if (order != null)
            {
                order.Status = OrderStatus.Fail;
                order.FailMessage = context.Message.Reason;
                await _appDbContext.SaveChangesAsync();
                _logger.LogInformation($"Order with id : {order.Id} changed status to {OrderStatus.Fail}");
            }
            else
            {
                _logger.LogInformation($"Order id : {context.Message.OrderId} not found!");
            }
        }
    }
}
