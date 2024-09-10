namespace groveale.Services
{
    public interface ISettingsService
    {
        string TenantId { get; }
        string TenantDomain { get; }
        string ClientId { get; }
        string ClientSecret { get; }
    }

    public class SettingsService : ISettingsService
    {
        public string TenantId => Environment.GetEnvironmentVariable("TenantId");
        public string TenantDomain => Environment.GetEnvironmentVariable("TenantDomain");
        public string ClientId => Environment.GetEnvironmentVariable("ClientId");
        public string ClientSecret => Environment.GetEnvironmentVariable("ClientSecret");
    }
}