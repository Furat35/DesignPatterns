using MassTransit;
using Microsoft.EntityFrameworkCore;
using Order.API.Models;
using Shared.Contracts;

namespace Order.API.Consumers
{
    public class OrderRequestCompletedEventComsumer : IConsumer<IOrderRequestCompletedEvent>
    {
        private readonly AppDbContext _appDbContext;
        private readonly ILogger<OrderRequestCompletedEventComsumer> _logger;

        public OrderRequestCompletedEventComsumer(ILogger<OrderRequestCompletedEventComsumer> logger, AppDbContext appDbContext)
        {
            _logger = logger;
            _appDbContext = appDbContext;
        }
        public async Task Consume(ConsumeContext<IOrderRequestCompletedEvent> context)
        {
            var order = await _appDbContext.Orders.FirstAsync(_ => _.Id == context.Message.OrderId);
            if (order != null)
            {
                order.Status = OrderStatus.Completed;
                await _appDbContext.SaveChangesAsync();
                _logger.LogInformation($"Order with id : {order.Id} changed status to {OrderStatus.Completed}");
            }
            else
            {
                _logger.LogInformation($"Order id : {context.Message.OrderId} not found!");
            }
        }
    }
}
