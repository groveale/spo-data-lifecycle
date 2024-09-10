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

    // Register your custom service here
    services.AddSingleton<IM365ActivityService, M365ActivityService>();


    })
    .Build();

host.Run();
