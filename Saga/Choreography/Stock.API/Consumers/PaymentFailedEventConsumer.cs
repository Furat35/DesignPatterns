using MassTransit;
using Microsoft.EntityFrameworkCore;
using Shared.Events;
using Stock.API.Models;

namespace Stock.API.Consumers
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
            foreach (var item in context.Message.OrderItems)
            {
                var stock = await _appDbContext.Stocks.FirstOrDefaultAsync(_ => _.ProductId == item.ProductId);
                if (stock != null)
                {
                    stock.Count += item.Count;
                    await _appDbContext.SaveChangesAsync();
                }
            }
            _logger.LogInformation($"Stock was released for Order Id : ({context.Message.OrderId})");
        }
    }
}
