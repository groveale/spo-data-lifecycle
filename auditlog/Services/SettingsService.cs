namespace groveale.Services
{
    public interface ISettingsService
    {
        string TenantId { get; }
        string TenantDomain { get; }
        string ClientId { get; }
        string ClientSecret { get; }
        string AuthGuid { get; }
        string StorageAccountUri { get; }
        string StorageAccountName { get; }
        string StorageAccountKey { get; }
        
    }

    public class SettingsService : ISettingsService
    {
        public string TenantId => Environment.GetEnvironmentVariable("TenantId");
        public string TenantDomain => Environment.GetEnvironmentVariable("TenantDomain");
        public string ClientId => Environment.GetEnvironmentVariable("ClientId");
        public string ClientSecret => Environment.GetEnvironmentVariable("ClientSecret");
        public string AuthGuid => Environment.GetEnvironmentVariable("AuthGuid");

        public string StorageAccountUri => Environment.GetEnvironmentVariable("StorageAccountUri");
        public string StorageAccountName => Environment.GetEnvironmentVariable("StorageAccountName");
        public string StorageAccountKey => Environment.GetEnvironmentVariable("StorageAccountKey");

    }
}