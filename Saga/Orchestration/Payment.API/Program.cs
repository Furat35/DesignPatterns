using MassTransit;
using Payment.API.Consumers;
using Shared.Consts;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMassTransit(configure =>
{
    configure.AddConsumer<StockReservedRequestPaymentConsumer>();
    configure.UsingRabbitMq((context, cfg) =>
    {
        cfg.ReceiveEndpoint(RabbitMqQueues.PaymentStockReservedRequestQueueName, e =>
        {
            e.ConfigureConsumer<StockReservedRequestPaymentConsumer>(context);
        });
    });
});
builder.Services.AddControllers();

var app = builder.Build();

app.UseAuthorization();

app.MapControllers();

app.Run();
