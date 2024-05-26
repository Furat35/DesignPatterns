using MassTransit;
using Shared.Consts;
using Shared.Contracts;
using Shared.Events;
using Shared.Messages;

namespace SageStateMachineWorkerService.Models
{
    public class OrderStateMachine : MassTransitStateMachine<OrderStateInstance>
    {
        public Event<IOrderCreatedRequestEvent> OrderCreatedRequestEvent { get; set; }
        public Event<IStockReservedEvent> StockReservedEvent { get; set; }
        public Event<IStockNotReservedEvent> StockNotReservedEvent { get; set; }
        public Event<IPaymentCompletedEvent> PaymentCompletedEvent { get; set; }
        public Event<IPaymentFailedEvent> PaymentFailedEvent { get; set; }
        public State OrderCreated { get; private set; }
        public State StockReserved { get; private set; }
        public State StockNotReserved { get; private set; }
        public State PaymentCompleted { get; private set; }
        public State PaymentFailed { get; private set; }

        public OrderStateMachine()
        {
            InstanceState(x => x.CurrentState);

            Event(() => OrderCreatedRequestEvent, y => y.CorrelateBy<int>(x => x.OrderId, z => z.Message.OrderId).SelectId(context => Guid.NewGuid()));
            Event(() => StockReservedEvent, y => y.CorrelateById(z => z.Message.CorrelationId));
            Event(() => StockNotReservedEvent, y => y.CorrelateById(z => z.Message.CorrelationId));
            Event(() => PaymentCompletedEvent, y => y.CorrelateById(z => z.Message.CorrelationId));

            Initially(When(OrderCreatedRequestEvent)
                .Then(context =>
            {
                context.Saga.BuyerId = context.Data.BuyerId;

                context.Saga.OrderId = context.Data.OrderId;
                context.Saga.CreatedDate = DateTime.Now;

                context.Saga.CardName = context.Data.Payment.CardName;
                context.Saga.CardNumber = context.Data.Payment.CardNumber;
                context.Saga.CVV = context.Data.Payment.CVV;
                context.Saga.Expiration = context.Data.Payment.Expiration;
                context.Saga.TotalPrice = context.Data.Payment.TotalPrice;
            })
                .Then(context => { Console.WriteLine($"OrderCreatedRequestEvent before :\n{context.Saga}"); })
                .Publish(context => new OrderCreatedEvent(context.Saga.CorrelationId) { OrderItems = context.Data.OrderItems })
                .TransitionTo(OrderCreated)
                .Then(context => { Console.WriteLine($"OrderCreatedRequestEvent after :\n{context.Saga}"); }));

            During(OrderCreated,
                  When(StockReservedEvent)
                      .TransitionTo(StockReserved)
                      .Send(new Uri($"queue:{RabbitMqQueues.PaymentStockReservedRequestQueueName}"), context =>
                          new StockReservedRequestPayment(context.Saga.CorrelationId)
                          {
                              OrderItems = context.Message.OrderItems,
                              Payment = new PaymentMessage()
                              {
                                  CardName = context.Saga.CardName,
                                  CardNumber = context.Saga.CardNumber,
                                  CVV = context.Saga.CVV,
                                  Expiration = context.Saga.Expiration,
                                  TotalPrice = context.Saga.TotalPrice
                              },
                              BuyerId = context.Saga.BuyerId
                          })
                      .Then(context => { Console.WriteLine($"StockReservedEvent After : {context.Saga}"); }),
                  When(StockNotReservedEvent)
                  .TransitionTo(StockNotReserved)
                      .Publish(context => new OrderRequestFailedEvent() { OrderId = context.Saga.OrderId, Reason = context.Message.Reason })
                      .Then(context => { Console.WriteLine($"StockReservedEvent After : {context.Saga}"); }));

            During(StockReserved,
                When(PaymentCompletedEvent)
                    .TransitionTo(PaymentCompleted)
                    .Publish(context => new OrderRequestCompletedEvent { OrderId = context.Saga.OrderId })
                    .Then(context => { Console.WriteLine($"PaymentCompletedEvent after :\n{context.Saga}"); })
                    .Finalize(),
                When(PaymentFailedEvent)
                    .Publish(context => new OrderRequestFailedEvent() { OrderId = context.Saga.OrderId, Reason = context.Message.Reason })
                    .Send(new Uri($"queue:{RabbitMqQueues.StockRollbackMessageQueueName}"), context => new StockRollbackMessage { OrderItems = context.Data.OrderItems })
                    .TransitionTo(PaymentFailed)
                    .Then(context => { Console.WriteLine($"PaymentFailedEvent after :\n{context.Saga}"); }));


            //SetCompletedWhenFinalized();
        }
    }
}
