using Azure.Storage;
using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Azure.Storage.Files.Shares;

namespace CoffeeNChillFunctions
{
    public class DownloadStaffDocument
    {
        private readonly ILogger _logger;
        private const string ConnectionString = "DefaultEndpointsProtocol=https;AccountName=coffeenchillstorage6;AccountKey=D7q752XFlgKjtr7+XuOA8fkxN8CMNduwvl84ew50Wv75phmtvbAd3m1v/qmXR2v5fwCN1Qsz2JnM+ASt9koapw==;EndpointSuffix=core.windows.net";
        private const string ShareName = "staff-docs";

        public DownloadStaffDocument(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<DownloadStaffDocument>();
        }

        [Function("DownloadStaffDocument")]
        public async Task<HttpResponseData> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "documents/download/{fileName}")] HttpRequestData req,
            string fileName)
        {
            _logger.LogInformation($"Downloading staff document: {fileName}");

            var shareClient = new ShareClient(ConnectionString, ShareName);
            var directory = shareClient.GetRootDirectoryClient();
            var fileClient = directory.GetFileClient(fileName);

            if (!await fileClient.ExistsAsync())
            {
                var notFound = req.CreateResponse(HttpStatusCode.NotFound);
                await notFound.WriteStringAsync($"File '{fileName}' not found.");
                return notFound;
            }

            var download = await fileClient.DownloadAsync();

            var response = req.CreateResponse(HttpStatusCode.OK);
            response.Headers.Add("Content-Disposition", $"attachment; filename=\"{fileName}\"");
            await download.Value.Content.CopyToAsync(response.Body);
            return response;
        }
    }
}