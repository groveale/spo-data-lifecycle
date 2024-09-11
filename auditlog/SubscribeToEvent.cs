using groveale.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace groveale
{
    public class SubscribeToEvent
    {
        private readonly ILogger<SubscribeToEvent> _logger;
        private readonly IM365ActivityService _m365ActivityService;

        public SubscribeToEvent(ILogger<SubscribeToEvent> logger, IM365ActivityService m365ActivityService)
        {
            _logger = logger;
            _m365ActivityService = m365ActivityService;
        }

        [Function("SubscribeToEvent")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequest req)
        {
            _logger.LogInformation("Subscribe to event function triggered.");

            // Get content type from query parameters
            string contentType = req.Query["contentType"];
            string webhookAddress = req.Query["webhookAddress"];
            string authId = req.Query["authId"];

            if (string.IsNullOrEmpty(contentType))
            {
                _logger.LogError("Audit content type is not provided.");
                return new BadRequestObjectResult("Audit content type is not provided.");
            }

            try
            {
                var response = await _m365ActivityService.SubscribeToAuditEventsAsync(contentType, webhookAddress);
                return new OkObjectResult(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating subscription.");
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }
        }
    }
}
