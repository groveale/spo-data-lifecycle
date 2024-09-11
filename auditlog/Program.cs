using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using groveale.Services;

var host = new HostBuilder()
    .ConfigureFunctionsWebApplication()
    .ConfigureServices(services => {
        services.AddApplicationInsightsTelemetryWorkerService();
        services.ConfigureFunctionsApplicationInsights();

        // Register HttpClient
        services.AddHttpClient();

        // Register the SettingsService
        services.AddSingleton<ISettingsService, SettingsService>();

        // Register the M365ActivityService
        services.AddSingleton<IM365ActivityService, M365ActivityService>();

        // Register the AzureTableService
        services.AddSingleton<IAzureTableService, AzureTableService>();

    })
    .Build();

host.Run();
