using Azure;
using Azure.Data.Tables;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace groverale
{
    public class RetentionEventReceiver
    {
        private readonly ILogger<RetentionEventReceiver> _logger;

        public RetentionEventReceiver(ILogger<RetentionEventReceiver> logger)
        {
            _logger = logger;
        }

        private static readonly string TableStorageUri = Environment.GetEnvironmentVariable("TableStorageUri");
        private static readonly string StorageAccountName = Environment.GetEnvironmentVariable("StorageAccountName");
        private static readonly string StorageAccountKey = Environment.GetEnvironmentVariable("StorageAccountKey");
        private static readonly string StorageTableName = Environment.GetEnvironmentVariable("StorageTableName");
        private static TableClient tableClient = new TableClient(
            new Uri(TableStorageUri),
            StorageTableName,
            new TableSharedKeyCredential(StorageAccountName, StorageAccountKey)
        );

        [Function("RetentionEventReceiver")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequest req)
        {
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            RetentionEvent retentionEvent = JsonConvert.DeserializeObject<RetentionEvent>(requestBody);

            if (retentionEvent.EventTime == default)
            {
                retentionEvent.EventTime = DateTime.Now;
            }

            _logger.LogInformation($"SiteUrl: {retentionEvent.SiteUrl}");

            // push to table storage
            // Insert retentionEvent into Azure Table Storage
            // Create a new table client
            var entity = new TableEntity(retentionEvent.SiteId.ToString(), retentionEvent.ItemUniqueId.ToString())
            {
                { "SiteUrl", retentionEvent.SiteUrl },
                { "ListName", retentionEvent.ListName },
                { "FolderPath", retentionEvent.FolderPath },
                { "DocumentName", retentionEvent.DocumentName },
                { "SiteId", retentionEvent.SiteId },
                { "WebId", retentionEvent.WebId },
                { "ListId", retentionEvent.ListId },
                { "ItemUniqueId", retentionEvent.ItemUniqueId },
                { "ListItemId", retentionEvent.ListItemId },
                { "LibraryName", retentionEvent.LibraryName },
                { "WorkloadName", retentionEvent.WorkloadName },
                { "ExistingLabelId", retentionEvent.ExistingLabelId },
                { "UserId", retentionEvent.UserId },
                { "EventTime", retentionEvent.EventTime }   
            };
        

            try
            {
                await tableClient.AddEntityAsync(entity);
            }
            catch (RequestFailedException e)
            {
                _logger.LogError(e.Message);
                return new BadRequestObjectResult($"Error inserting into table. {e.Message}");
            }

            return new OkObjectResult("retentionEvent inserted successfully");

        }
    }
}
