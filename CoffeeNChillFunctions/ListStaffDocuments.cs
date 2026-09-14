using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Azure.Storage.Files.Shares;

namespace CoffeeNChillFunctions
{
    public class ListStaffDocuments
    {
        private readonly ILogger _logger;
        private const string ConnectionString = "UseDevelopmentStorage=true";
        private const string ShareName = "staff-docs";

        public ListStaffDocuments(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<ListStaffDocuments>();
        }

        [Function("ListStaffDocuments")]
        public async Task<HttpResponseData> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "documents")] HttpRequestData req)
        {
            _logger.LogInformation("Listing staff documents.");

            var shareClient = new ShareClient(ConnectionString, ShareName);
            await shareClient.CreateIfNotExistsAsync();
            var directory = shareClient.GetRootDirectoryClient();

            var files = new List<object>();
            await foreach (var item in directory.GetFilesAndDirectoriesAsync())
            {
                if (!item.IsDirectory)
                {
                    var fileClient = directory.GetFileClient(item.Name);
                    var props = await fileClient.GetPropertiesAsync();
                    files.Add(new
                    {
                        FileName = item.Name,
                        SizeBytes = props.Value.ContentLength,
                        LastModified = props.Value.LastModified
                    });
                }
            }

            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(files);
            return response;
        }
    }
}