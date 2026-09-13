using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CoffeeNChillFunctions;

public class CreateMenuItem
{
    private readonly ILogger<CreateMenuItem> _logger;

    public CreateMenuItem(ILogger<CreateMenuItem> logger)
    {
        _logger = logger;
    }

    [Function("CreateMenuItem")]
    public IActionResult Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequest req)
    {
        _logger.LogInformation("C# HTTP trigger function processed a request.");
        return new OkObjectResult("Welcome to Azure Functions!");
    }
}
