using MassTransit;
using Microsoft.EntityFrameworkCore;
using Order.API.Consumers;
using Order.API.Models;
using Shared.Consts;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddControllers();
builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("SqlConnection"));
});

builder.Services.AddMassTransit((configure) =>
{
    EndpointConvention.Map<Shared.Events.OrderCreatedRequestEvent>(new Uri($"rabbitmq://localhost/{RabbitMqQueues.OrderSaga}"));

    configure.AddConsumer<OrderRequestCompletedEventComsumer>();
    configure.AddConsumer<OrderRequestFailedEventConsumer>();

    configure.UsingRabbitMq((context, cfg) =>
    {
        cfg.ReceiveEndpoint(RabbitMqQueues.OrderRequestCompleted_EventQueueName, e =>
        {
            e.ConfigureConsumer<OrderRequestCompletedEventComsumer>(context);
        });
        cfg.ReceiveEndpoint(RabbitMqQueues.OrderRequestFailed_EventQueueName, e =>
        {
            e.ConfigureConsumer<OrderRequestFailedEventConsumer>(context);
        });

    });
});

var app = builder.Build();


app.UseAuthorization();

app.MapControllers();

app.Run();
