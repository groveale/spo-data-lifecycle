using groveale.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace groveale
{
    public class GetNotifications
    {
        private readonly ILogger<GetNotifications> _logger;
        private readonly IM365ActivityService _m365ActivityService;

        private readonly IAzureTableService _azureTableService;

        public GetNotifications(ILogger<GetNotifications> logger, IM365ActivityService m365ActivityService, IAzureTableService azureTableService)
        {
            _logger = logger;
            _m365ActivityService = m365ActivityService;
            _azureTableService = azureTableService;
        }

        [Function("GetNotifications")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequest req)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request.");

            // Get content type from query parameters
            string contentType = req.Query["contentType"];
            if (string.IsNullOrEmpty(contentType))
            {
                _logger.LogError("Audit content type is not provided.");
                return new BadRequestObjectResult("Audit content type is not provided.");
            }

            try
            {
                var notificationResponses = await _m365ActivityService.GetAvailableNotificationsAsync(contentType);

                // process the response
                var newLists = await _m365ActivityService.GetListCreatedNotificationsAsync(notificationResponses);

                // store the new lists in the table
                foreach (var list in newLists)
                {
                    await _azureTableService.AddListCreationRecordAsync(list);
                }

                return new OkObjectResult(newLists);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting notifications.");
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }
        }
    }
}
