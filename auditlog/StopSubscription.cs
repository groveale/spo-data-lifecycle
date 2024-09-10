using groveale.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace groveale
{
    public class StopSubscription
    {
        private readonly ILogger<StopSubscription> _logger;
        private readonly IM365ActivityService _m365ActivityService;

        public StopSubscription(ILogger<StopSubscription> logger, IM365ActivityService m365ActivityService)
        {
            _logger = logger;
            _m365ActivityService = m365ActivityService;
        }

        [Function("StopSubscription")]
        public async Task<IActionResult> RunAsync([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequest req)
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
                var stopped = await _m365ActivityService.StopSubscriptionAsync(contentType);
                return new OkObjectResult(stopped);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error stopping subscriptions.");
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }
        }
    }
}
