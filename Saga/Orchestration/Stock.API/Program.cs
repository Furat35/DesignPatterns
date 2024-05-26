using MassTransit;
using Microsoft.EntityFrameworkCore;
using Shared.Consts;
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
    configure.AddConsumer<StockRollbackMessageConsumer>();
    configure.UsingRabbitMq((context, cfg) =>
    {
        cfg.ReceiveEndpoint(RabbitMqQueues.Stock_OrderCreated_EventQueueName, e =>
        {
            e.ConfigureConsumer<OrderCreatedEventConsumer>(context);
        });
        cfg.ReceiveEndpoint(RabbitMqQueues.StockRollbackMessageQueueName, e =>
        {
            e.ConfigureConsumer<StockRollbackMessageConsumer>(context);
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
