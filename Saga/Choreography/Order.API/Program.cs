using MassTransit;
using Microsoft.EntityFrameworkCore;
using Order.API.Consumers;
using Order.API.Models;
using Shared.Consts;
using Shared.Events;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddControllers();
builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("SqlConnection"));
});

builder.Services.AddMassTransit((configure) =>
{
    configure.AddConsumer<PaymentCompletedEventConsumer>();
    configure.AddConsumer<PaymentFailedEventConsumer>();
    configure.AddConsumer<StockNotReservedEventConsumer>();

    configure.UsingRabbitMq((context, cfg) =>
    {
        //cfg.Host(builder.Configuration.GetConnectionString("RabbitMq"));
        cfg.ReceiveEndpoint(RabbitMqQueues.OrderPaymentCompleted_EventQueueName, config =>
        {
            config.ConfigureConsumer<PaymentCompletedEventConsumer>(context);
        });
        cfg.ReceiveEndpoint(RabbitMqQueues.OrderPaymentFailed_EventQueueName, config =>
        {
            config.ConfigureConsumer<PaymentFailedEventConsumer>(context);
        });
        cfg.ReceiveEndpoint(RabbitMqQueues.OrderStockNotReserved_EventQueueName, config =>
        {
            config.ConfigureConsumer<StockNotReservedEventConsumer>(context);
        });
    });
});

var app = builder.Build();


app.UseAuthorization();

app.MapControllers();

app.Run();
