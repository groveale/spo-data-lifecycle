using groveale.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace groveale
{
    public class ReceiveEvents
    {
        private readonly ILogger<ReceiveEvents> _logger;
        private readonly ISettingsService _settingsService;
        private readonly IM365ActivityService _m365ActivityService;
        private readonly IAzureTableService _azureTableService; 

        public ReceiveEvents(ILogger<ReceiveEvents> logger, ISettingsService settingsService, IM365ActivityService m365ActivityService, IAzureTableService azureTableService)
        {
            _logger = logger;
            _settingsService = settingsService;
            _m365ActivityService = m365ActivityService;
            _azureTableService = azureTableService;

        }

        [Function("ReceiveEvents")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequest req)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request.");

            // validate that the headers contains the correct Webhook-AuthID and matches the configured value
            var authId = req.Headers["Webhook-AuthID"];
            if (string.IsNullOrEmpty(authId) || authId != _settingsService.AuthGuid)
            {
                _logger.LogError("Invalid Webhook-AuthID header.");
                return new BadRequestObjectResult("Invalid Webhook-AuthID header.");
            }

            // Parse the request body to extract the validation code from the payload
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var payload = JObject.Parse(requestBody);
            var validationCodeFromPayload = payload["validationCode"]?.ToString();

            // If the validation code is present, validate it against the Webhook-ValidationCode header
            // This is M365 initial validation request
            if (!string.IsNullOrEmpty(validationCodeFromPayload))
            {
                var validationHeaderCode = req.Headers["Webhook-ValidationCode"];
                if (validationHeaderCode != validationCodeFromPayload)
                {
                    _logger.LogError("Invalid Webhook-ValidationCode header.");
                    return new BadRequestObjectResult("Invalid Webhook-ValidationCode header.");
                }
                return new OkResult();
            }

            try
            {
                // Deserialize the request body into a list of NotificationResponse objects
                var notifications = JsonConvert.DeserializeObject<List<NotificationResponse>>(requestBody);

                // process the response
                var newLists = await _m365ActivityService.GetListCreatedNotificationsAsync(notifications);

                // store the new lists in the table
                foreach (var list in newLists)
                {
                    await _azureTableService.AddListCreationRecordAsync(list);
                }

                return new OkResult();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing notifications.");
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }
        }
    }
}
