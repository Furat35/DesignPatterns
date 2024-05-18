using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Order.API.DTOs;
using Order.API.Models;
using Shared.Events;
using Shared.Messages;

namespace Order.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly AppDbContext _appDbContext;
        private readonly IPublishEndpoint _publishEndpoint;
        public OrdersController(AppDbContext appDbContext, IPublishEndpoint publishEndpoint)
        {
            _appDbContext = appDbContext;
            _publishEndpoint = publishEndpoint;
        }

        [HttpPost]
        public async Task<IActionResult> Create(OrderCreateDto orderCreate)
        {
            var newOrder = new Models.Order
            {
                BuyerId = orderCreate.BuyerId,
                Status = OrderStatus.Suspend,
                Address = new() { Line = orderCreate.Address.Line, Province = orderCreate.Address.Line, District = orderCreate.Address.District },
                CreatedDate = DateTime.Now
            };
            orderCreate.OrderItems.ForEach(item =>
            {
                newOrder.Items.Add(new()
                {
                    Price = item.Price,
                    ProductId = item.ProductId,
                    Count = item.Count,
                });
            });
            await _appDbContext.AddAsync(newOrder);
            await _appDbContext.SaveChangesAsync();

            var orderCreateEvent = new OrderCreatedEvent
            {
                BuyerId = orderCreate.BuyerId,
                OrderId = newOrder.Id,
                Payment = new PaymentMessage
                {
                    CardName = orderCreate.Payment.CardName,
                    CardNumber = orderCreate.Payment.CardNumber,
                    Expiration = orderCreate.Payment.Expiration,
                    CVV = orderCreate.Payment.CVV,
                    TotalPrice = orderCreate.OrderItems.Sum(item => item.Price * item.Count)
                }
            };
            orderCreate.OrderItems.ForEach(item =>
            {
                orderCreateEvent.OrderItems.Add(new() { Count = item.Count, ProductId = item.ProductId });
            });
            await _publishEndpoint.Publish(orderCreateEvent);

            return Ok();
        }
    }
}
