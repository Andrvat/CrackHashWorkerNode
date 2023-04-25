using DataContracts.Dto;
using DataContracts.MassTransit;
using MassTransit;
using Worker.Logic;

namespace Worker.Controllers;

public class CrackHashWorkerController
{
    private CrackHashWorker _crackHashWorker;
    private MessageService<CrackHashManagerRequestDto> _messageService;
    private IBus _bus;
    
    private HttpClient _httpClient;
    
    public CrackHashWorkerController(
        CrackHashWorker crackHashWorker, 
        MessageService<CrackHashManagerRequestDto> messageService, 
        IBus bus)
    {
        _crackHashWorker = crackHashWorker;
        _messageService = messageService;
        _bus = bus;
        
        var httpClientHandler = new HttpClientHandler();
        httpClientHandler.ServerCertificateCustomValidationCallback = (_, _, _, _) => true;
        _httpClient = new HttpClient(httpClientHandler);
        _httpClient.BaseAddress = new Uri("http://manager:5180/");
    }

    public async void RunCrackHashTask()
    {
        var workerTaskInfo = _messageService.GetMessage();
        Console.WriteLine($"Data from RabbitMq: {workerTaskInfo}");
        
        var words = await _crackHashWorker.Run(workerTaskInfo);
        var wordsToString = string.Join(", ", words);
        Console.WriteLine($"Calculated words: {wordsToString}");
        
        try
        {
            await SendResponseToManager(workerTaskInfo, words);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }

    private async Task SendResponseToManager(CrackHashManagerRequestDto workerTaskInfo, List<string> words)
    {
        var crackHashWorkerResponseDto = new CrackHashWorkerResponseDto
        {
            RequestId = workerTaskInfo.RequestId,
            PartNumber = workerTaskInfo.PartNumber,
            Words = words.ToArray()
        };
        
        Console.WriteLine($"Send data to RabbitMQ: {_bus.Address}. Data: {crackHashWorkerResponseDto}");
        await _bus.Publish<ITaskFinished>(crackHashWorkerResponseDto);
        
        var managerRequestPath = "/internal/api/manager/hash/crack/request";
        Console.WriteLine($"Send request to manager by path: {managerRequestPath}");
        await _httpClient.PatchAsync(managerRequestPath, null);
    }
}