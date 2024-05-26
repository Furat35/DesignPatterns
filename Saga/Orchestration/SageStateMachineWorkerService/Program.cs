using MassTransit;
using Microsoft.EntityFrameworkCore;
using SageStateMachineWorkerService.Models;
using Shared.Consts;
using System.Reflection;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddMassTransit(cfg =>
{
    cfg.AddSagaStateMachine<OrderStateMachine, OrderStateInstance>()
    .EntityFrameworkRepository(opt =>
    {
        opt.AddDbContext<DbContext, OrderStateDbContext>((provider, optionsBuilder) =>
        {
            optionsBuilder.UseSqlServer(builder.Configuration.GetConnectionString("SqlConnection"), opt =>
            {
                opt.MigrationsAssembly(Assembly.GetExecutingAssembly().GetName().Name);
            });
        });
    });
    cfg.AddBus(provider => Bus.Factory.CreateUsingRabbitMq(configure =>
    {
        configure.ReceiveEndpoint(RabbitMqQueues.OrderSaga, e =>
        {
            e.ConfigureSaga<OrderStateInstance>(provider);
        });
    }));
});

var host = builder.Build();
host.Run();
