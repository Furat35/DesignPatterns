using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Order.API.DTOs;
using Order.API.Models;
using Shared.Consts;
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
        private readonly ISendEndpointProvider _sendEndpoint;
        public OrdersController(AppDbContext appDbContext, IPublishEndpoint publishEndpoint, ISendEndpointProvider sendEndpoint)
        {
            _appDbContext = appDbContext;
            _publishEndpoint = publishEndpoint;
            _sendEndpoint = sendEndpoint;
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

            var orderCreateRequestEvent = new OrderCreatedRequestEvent
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
                orderCreateRequestEvent.OrderItems.Add(new() { Count = item.Count, ProductId = item.ProductId });
            });
            var sendEndpoint = await _sendEndpoint.GetSendEndpoint(new Uri($"queue:{RabbitMqQueues.OrderSaga}"));
            await sendEndpoint.Send(orderCreateRequestEvent);

            return Ok();
        }
    }
}
