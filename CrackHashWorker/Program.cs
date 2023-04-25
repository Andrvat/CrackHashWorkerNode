using DataContracts.Dto;
using DataContracts.MassTransit;
using MassTransit;
using Worker.Consumers;
using Worker.Logic;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddScoped<CrackHashWorker>();
builder.Services.AddScoped<WorkerTaskSentConsumer>();
builder.Services.AddSingleton<MessageService<CrackHashManagerRequestDto>>();

builder.Services.AddMassTransit(x => { 
    x.UsingRabbitMq((busRegistrationContext, busFactoryConfigurator) =>
    {
        busFactoryConfigurator.Host(new Uri("rabbitmq://rabbitmq:5672/"), h =>
        {
            h.Username("guest");
            h.Password("guest");
        });
        
        busFactoryConfigurator.ReceiveEndpoint("worker-task-sent", e =>
        {
            e.Consumer<WorkerTaskSentConsumer>(busRegistrationContext);
            e.PurgeOnStartup = true;
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