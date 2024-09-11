using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using groveale.Models;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace groveale.Services
{
    public interface IM365ActivityService
    {
        Task AuthenticateAsync();
        Task<ApiSubscription> SubscribeToAuditEventsAsync(string configuredContentType, string webhookAddress);
        Task<ApiSubscription[]> GetExistingSubscriptionsAsync();

        Task<string> StopSubscriptionAsync(string configuredContentType);

        Task<List<ListAuditObj>> GetListCreatedNotificationsAsync(List<NotificationResponse> notifications);

        Task<List<NotificationResponse>> GetAvailableNotificationsAsync(string configuredContentType);
        
    }

    public class M365ActivityService : IM365ActivityService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<M365ActivityService> _logger;
        private readonly ISettingsService _settingsService;
        private string _accessToken;


        public M365ActivityService(HttpClient httpClient, ILogger<M365ActivityService> logger, ISettingsService settingsService)
        {
            _httpClient = httpClient;
            _logger = logger;
            _settingsService = settingsService;
            _accessToken = string.Empty;
        }

        public async Task AuthenticateAsync()
        {
            if (!string.IsNullOrEmpty(_accessToken))
            {
                _logger.LogInformation("Already authenticated.");
                return;
            }

            var tokenEndpoint = $"https://login.microsoftonline.com/{_settingsService.TenantDomain}/oauth2/token?api-version=1.0";
            var requestBody = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("grant_type", "client_credentials"),
                new KeyValuePair<string, string>("resource", "https://manage.office.com"),
                new KeyValuePair<string, string>("client_id", _settingsService.ClientId),
                new KeyValuePair<string, string>("client_secret", _settingsService.ClientSecret)
            });

            var response = await _httpClient.PostAsync(tokenEndpoint, requestBody);
            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync();
            var tokenResponse = JsonConvert.DeserializeObject<TokenResponse>(responseContent);
            _accessToken = tokenResponse.AccessToken;

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);

            _logger.LogInformation("Authenticated successfully.");
        }

        public async Task<ApiSubscription> SubscribeToAuditEventsAsync(string configuredContentType, string webhookAddress)
        {
            if (string.IsNullOrEmpty(_accessToken))
            {
                await AuthenticateAsync();
            }

            // Check if the subscription already exists
            var existingSubscriptions = await GetExistingSubscriptionsAsync();

            foreach (var subscription in existingSubscriptions)
            {
                if (subscription.ContentType.Equals(configuredContentType, StringComparison.OrdinalIgnoreCase))
                {
                    _logger.LogInformation($"Subscription for '{configuredContentType}' already exists.");
                    return subscription;
                }
            }

            // Create a new subscription if it doesn't exist
            try
            {
                _logger.LogInformation("Creating subscription for content-type {0}. Need to wait a few seconds before it can be accessed...", configuredContentType);
                var url = $"https://manage.office.com/api/v1.0/{_settingsService.TenantId}/activity/feed/subscriptions/start?ContentType={configuredContentType}&PublisherIdentifier={_settingsService.TenantId}";

                var payload = new
                {
                    webhook = new
                    {
                        address = webhookAddress,
                        authId = _settingsService.AuthGuid,
                        expiration = ""
                    }
                };

                var content = new StringContent(JsonConvert.SerializeObject(payload), System.Text.Encoding.UTF8, "application/json");

                _logger.LogInformation("+{0}", url);
                var response = await _httpClient.PostAsync(url, content);

                try
                {
                    response.EnsureSuccessStatusCode();
                }
                catch (HttpRequestException)
                {
                    _logger.LogInformation("Can't create subscription. Check service-account permissions to Office 365 Activity API & that audit-log is turned on for tenant.");
                    _logger.LogInformation("https://docs.microsoft.com/en-gb/microsoft-365/compliance/turn-audit-log-search-on-or-off?view=o365-worldwide");
                    throw;
                }

                _logger.LogInformation($"Subscription for '{configuredContentType}' has been created.");
                var responseBody = await response.Content.ReadAsStringAsync();
                var subscription = JsonConvert.DeserializeObject<ApiSubscription>(responseBody);
                return subscription;
            }
            catch (HttpRequestException ex)
            {
                // If we can't create it report the error
                _logger.LogInformation($"Subscription for '{configuredContentType}' could not be found or created - {ex.Message}. Check the configuration file & app permissions in Azure AD.");
                throw;
            }
        }

        public async Task<ApiSubscription[]> GetExistingSubscriptionsAsync()
        {
            if (string.IsNullOrEmpty(_accessToken))
            {
                await AuthenticateAsync();
            }

            var listUrl = $"https://manage.office.com/api/v1.0/{_settingsService.TenantId}/activity/feed/subscriptions/list";
            var listResponse = await _httpClient.GetAsync(listUrl);
            _logger.LogInformation("Reading existing Office 365 Activity API subscriptions...");

            var responseBody = await listResponse.Content.ReadAsStringAsync();
            listResponse.EnsureSuccessStatusCode();

            return JsonConvert.DeserializeObject<ApiSubscription[]>(responseBody) ?? new List<ApiSubscription>().ToArray();
        }

        public async Task<string> StopSubscriptionAsync(string configuredContentType)
        {

            if (string.IsNullOrEmpty(_accessToken))
            {
                await AuthenticateAsync();
            }

            try
            {
                // Construct the full URL with query parameters
                var url = $"https://manage.office.com/api/v1.0/{_settingsService.TenantId}/activity/feed/subscriptions/stop?contentType={configuredContentType}&PublisherIdentifier={_settingsService.TenantId}";

                // Send a POST request to the endpoint
                var response = await _httpClient.PostAsync(url, null);

                // Ensure the request was successful
                response.EnsureSuccessStatusCode();

                // Log the success
                _logger.LogInformation($"Subscription for '{configuredContentType}' has been stopped.");
                return "Subscription stopped";
            } 
            catch (HttpRequestException ex)
            {
                // If we can't stop it report the error
                _logger.LogInformation($"Subscription for '{configuredContentType}' could not be stopped - {ex.Message}. Check the configuration file & app permissions in Azure AD.");
                throw;
            }
        }
    
        public async Task<List<ListAuditObj>> GetListCreatedNotificationsAsync(List<NotificationResponse> notifications)
        {
            if (string.IsNullOrEmpty(_accessToken))
            {
                await AuthenticateAsync();
            }

            // You will need to change this to match the object you are deserializing
            List<ListAuditObj> newLists = new List<ListAuditObj>();

            // Process each notification
            foreach (NotificationResponse notification in notifications)
            {
                // Add your processing logic here
                // For example, log the notification details
                _logger.LogInformation($"Processing notification for TenantId: {notification.TenantId}, ContentId: {notification.ContentId}");
                _logger.LogInformation($"Notification Id: {notification.ContentId}");

                var url = notification.ContentUri;
                var response = await _httpClient.GetAsync(url);

                using (var stream = await response.Content.ReadAsStreamAsync())
                using (var streamReader = new StreamReader(stream))
                using (var jsonReader = new JsonTextReader(streamReader))
                {
                    var serializer = new JsonSerializer();
                    while (jsonReader.Read())
                    {
                        if (jsonReader.TokenType == JsonToken.StartObject)
                        {
                            dynamic notificationResponse = serializer.Deserialize<object>(jsonReader);
                            
                            // This is an example of how to filter notifications based on the operation
                            // If you are interested in a specific operation, you can filter the notifications here
                            // You may also need to create a class to build the operation object
                            if (notificationResponse.Operation == "ListCreated")
                            {
                                // Process the filtered notification
                                var listObj = new ListAuditObj
                                {
                                    AuditLogId = notificationResponse.Id,
                                    ListName = notificationResponse.ListName,
                                    ListUrl = notificationResponse.ListUrl,
                                    ObjectId = notificationResponse.ObjectId,
                                    ListBaseTemplateType = notificationResponse.ListBaseTemplateType,
                                    ListBaseType = notificationResponse.ListBaseType,
                                    CreationTime = notificationResponse.CreationTime
                                };

                                _logger.LogInformation($"List Creation: {listObj.ListName}");
                                newLists.Add(listObj);
                            }
                        }
                    }
                }

            }

            // Log the number of notifications processed
            _logger.LogInformation($"Processed {notifications.Count} notifications.");
            _logger.LogInformation($"Found {newLists.Count} new lists.");

            return newLists;
        }

        // Not used by the webhook - just for testing
        public async Task<List<NotificationResponse>> GetAvailableNotificationsAsync(string configuredContentType)
        {
            if (string.IsNullOrEmpty(_accessToken))
            {
                await AuthenticateAsync();
            }

            var url = $"https://manage.office.com/api/v1.0/{_settingsService.TenantId}/activity/feed//subscriptions/content?contentType={configuredContentType}&PublisherIdentifier={_settingsService.TenantId}";
            var response = await _httpClient.GetAsync(url);

            try
            {
                response.EnsureSuccessStatusCode();
            }
            catch (HttpRequestException ex)
            {
                _logger.LogInformation($"Can't get notifications - {ex.Message}");
                throw;
            }

            var responseBody = await response.Content.ReadAsStringAsync();
            //_logger.LogInformation(responseBody);

            return JsonConvert.DeserializeObject<List<NotificationResponse>>(responseBody);
        }
    }
}