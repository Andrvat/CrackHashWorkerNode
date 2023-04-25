using System.Collections.Concurrent;
using System.Xml.Serialization;
using DataContracts;
using DataContracts.Dto;
using DataContracts.MassTransit;
using MassTransit;

namespace Worker.Consumers;

public class WorkerTaskSentConsumer : IConsumer<ISendWorkerTask>
{
    private readonly MessageService<CrackHashManagerRequestDto> _messageService;

    public WorkerTaskSentConsumer(MessageService<CrackHashManagerRequestDto> messageService)
    {
        _messageService = messageService;
    }

    public Task Consume([XmlElement] ConsumeContext<ISendWorkerTask> context)
    {
        var message = context.Message;
        var managerRequest = MapperConfig.GetAutomapperInstance().Map<CrackHashManagerRequestDto>(message);
        Console.WriteLine($"\nCONSUMER Consume\n {managerRequest}");
        _messageService.AddMessage(managerRequest);
        return Task.CompletedTask;
    }
}