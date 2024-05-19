using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Shared.Consts;
using Shared.Events;
using Stock.API.Consumers;
using Stock.API.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseInMemoryDatabase("StockDb");
});
builder.Services.AddMassTransit(configure =>
{
    configure.AddConsumer<OrderCreatedEventConsumer>();
    configure.AddConsumer<PaymentFailedEventConsumer>();
    EndpointConvention.Map<StockReservedEvent>(new Uri($"queue:{RabbitMqQueues.StockReserved_EventQueueName}"));

    configure.UsingRabbitMq((context, cfg) =>
    {
        //cfg.Host(builder.Configuration.GetConnectionString("RabbitMq"));
        cfg.ReceiveEndpoint(RabbitMqQueues.Stock_OrderCreated_EventQueueName, config =>
        {
            config.ConfigureConsumer<OrderCreatedEventConsumer>(context);
        });
        cfg.ReceiveEndpoint(RabbitMqQueues.StockPaymentFailed_EventQueueName, config =>
        {
            config.ConfigureConsumer<PaymentFailedEventConsumer>(context);
        });
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    using (var scope = app.Services.CreateScope())
    {
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        context.Add(new Stock.API.Models.Stock { Id = 1, ProductId = 2, Count = 113 });
        context.Add(new Stock.API.Models.Stock { Id = 2, ProductId = 5, Count = 112 });
        await context.SaveChangesAsync();
    }
}
app.UseAuthorization();

app.MapControllers();

app.Run();
