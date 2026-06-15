using System.Threading.Tasks;

namespace Isekai12Realms.RemoteConfig
{
    public interface IRemoteConfigService
    {
        bool IsAvailable { get; }
        Task InitializeAsync();
        Task FetchAndActivateAsync();
        string GetString(string key, string defaultValue);
        int GetInt(string key, int defaultValue);
        float GetFloat(string key, float defaultValue);
        bool GetBool(string key, bool defaultValue);
    }
}
