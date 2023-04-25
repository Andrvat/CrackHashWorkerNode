using System.Collections.Concurrent;
using System.Xml.Serialization;
using DataContracts;
using DataContracts.Dto;
using DataContracts.MassTransit;
using MassTransit;
using Worker.Controllers;

namespace Worker.Consumers;

public class WorkerTaskSentConsumer : IConsumer<ISendWorkerTask>
{
    private readonly MessageService<CrackHashManagerRequestDto> _messageService;
    private readonly CrackHashWorkerController _crackHashWorkerController;

    public WorkerTaskSentConsumer(MessageService<CrackHashManagerRequestDto> messageService, CrackHashWorkerController crackHashWorkerController)
    {
        _messageService = messageService;
        _crackHashWorkerController = crackHashWorkerController;
    }

    public Task Consume([XmlElement] ConsumeContext<ISendWorkerTask> context)
    {
        var message = context.Message;
        var managerRequest = MapperConfig.GetAutomapperInstance().Map<CrackHashManagerRequestDto>(message);
        Console.WriteLine($"\nCONSUMER Consume\n {managerRequest}");
        _messageService.AddMessage(managerRequest);
        
        _crackHashWorkerController.RunCrackHashTask();
        
        return Task.CompletedTask;
    }
}