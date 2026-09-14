using System.Net;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Azure.Storage.Files.Shares;

namespace CoffeeNChillFunctions
{
    public class UploadStaffDocument
    {
        private readonly ILogger _logger;
        private const string ConnectionString = "UseDevelopmentStorage=true";
        private const string ShareName = "staff-docs";

        public UploadStaffDocument(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<UploadStaffDocument>();
        }

        [Function("UploadStaffDocument")]
        public async Task<HttpResponseData> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "documents/upload")] HttpRequestData req)
        {
            _logger.LogInformation("Uploading a staff document.");

            // Parse the multipart/form-data body to get the uploaded file
            var boundary = GetBoundary(req.Headers.GetValues("Content-Type").First());
            var reader = new MultipartReader(boundary, req.Body);
            var section = await reader.ReadNextSectionAsync();

            if (section == null)
            {
                var badResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                await badResponse.WriteStringAsync("No file found in the request.");
                return badResponse;
            }

            var fileName = section.ContentDisposition?.Split("filename=")[1].Trim('"') ?? "unnamed-file";

            var shareClient = new ShareClient(ConnectionString, ShareName);
            await shareClient.CreateIfNotExistsAsync();
            var directory = shareClient.GetRootDirectoryClient();
            var fileClient = directory.GetFileClient(fileName);

            using var memoryStream = new MemoryStream();
            await section.Body.CopyToAsync(memoryStream);
            memoryStream.Position = 0;

            await fileClient.CreateAsync(memoryStream.Length);
            await fileClient.UploadRangeAsync(new Azure.HttpRange(0, memoryStream.Length), memoryStream);

            var response = req.CreateResponse(HttpStatusCode.Created);
            await response.WriteStringAsync($"Uploaded '{fileName}' successfully.");
            return response;
        }

        private static string GetBoundary(string contentType)
        {
            var elements = contentType.Split(' ');
            var boundaryElement = elements.First(x => x.StartsWith("boundary="));
            return boundaryElement.Substring("boundary=".Length).Trim('"');
        }
    }
}