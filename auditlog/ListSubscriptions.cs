using groveale.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace groveale
{
    public class ListSubscriptions
    {
        private readonly ILogger<ListSubscriptions> _logger;
        private readonly IM365ActivityService _m365ActivityService;

        public ListSubscriptions(ILogger<ListSubscriptions> logger, IM365ActivityService m365ActivityService)
        {
            _logger = logger;
            _m365ActivityService = m365ActivityService;
        }

        [Function("ListSubscriptions")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequest req)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request.");

            try
            {
                var subscriptions = await _m365ActivityService.GetExistingSubscriptionsAsync();
                return new OkObjectResult(subscriptions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error listing subscriptions.");
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }
        }
    }
}
