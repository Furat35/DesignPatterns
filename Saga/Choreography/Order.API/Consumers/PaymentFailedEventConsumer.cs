using MassTransit;
using Microsoft.EntityFrameworkCore;
using Order.API.Models;
using Shared.Events;

namespace Order.API.Consumers
{
    public class PaymentFailedEventConsumer : IConsumer<PaymentFailedEvent>
    {
        private readonly AppDbContext _appDbContext;
        private readonly ILogger<PaymentFailedEventConsumer> _logger;

        public PaymentFailedEventConsumer(ILogger<PaymentFailedEventConsumer> logger, AppDbContext appDbContext)
        {
            _logger = logger;
            _appDbContext = appDbContext;
        }

        public async Task Consume(ConsumeContext<PaymentFailedEvent> context)
        {
            var order = await _appDbContext.Orders.FirstOrDefaultAsync(_ => _.Id == context.Message.OrderId);
            if (order != null)
            {
                order.Status = OrderStatus.Fail;
                order.FailMessage = context.Message.Message;
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
