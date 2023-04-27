using DataContracts.Dto;
using DataContracts.MassTransit;
using MassTransit;
using Worker.Consumers;
using Worker.Controllers;
using Worker.Logic;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddScoped<CrackHashWorker>();
builder.Services.AddScoped<WorkerTaskSentConsumer>();
builder.Services.AddScoped<CrackHashWorkerController>();
builder.Services.AddSingleton<MessageService<CrackHashManagerRequestDto>>();

builder.Services.AddMassTransit(x => { 
    x.UsingRabbitMq((busRegistrationContext, busFactoryConfigurator) =>
    {
        busFactoryConfigurator.Host(new Uri(Environment.GetEnvironmentVariable("RABBITMQ_3_URI")!), h =>
        {
            h.Username(Environment.GetEnvironmentVariable("RABBITMQ_3_LOGIN")!);
            h.Password(Environment.GetEnvironmentVariable("RABBITMQ_3_PASSWORD")!);
            h.Heartbeat(TimeSpan.Zero);
        });
        
        // busFactoryConfigurator.Publish<ITaskFinished>(x => { x.ExchangeType = "direct";});

        busFactoryConfigurator.ReceiveEndpoint("worker-task-sent", e =>
        {
            // e.Bind("worker-task-sent-exchange", x =>
            // {
            //     x.Durable = true;
            //     x.AutoDelete = false;
            //     x.ExchangeType = "direct";
            // });
            // e.Bind<ISendWorkerTask>();
            e.Consumer<WorkerTaskSentConsumer>(busRegistrationContext);
            e.PurgeOnStartup = false;
            e.Durable = true;
            e.AutoDelete = false;
        });
    });
});

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();