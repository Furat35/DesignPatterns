using MassTransit;
using Payment.API.Consumers;
using Shared.Consts;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMassTransit(configure =>
{
    configure.AddConsumer<StockReservedEventConsumer>();

    configure.UsingRabbitMq((context, cfg) =>
    {
        //cfg.Host(builder.Configuration.GetConnectionString("RabbitMq"));
        cfg.ReceiveEndpoint(RabbitMqQueues.StockReserved_EventQueueName, config =>
        {
            config.ConfigureConsumer<StockReservedEventConsumer>(context);
        });
    });
});
builder.Services.AddControllers();

var app = builder.Build();

app.UseAuthorization();

app.MapControllers();

app.Run();
