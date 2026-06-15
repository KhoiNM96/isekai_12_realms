using System.Threading.Tasks;

namespace Isekai12Realms.RemoteConfig
{
    public class MockRemoteConfigService : IRemoteConfigService
    {
        public bool IsAvailable => false;
        public Task InitializeAsync() => Task.CompletedTask;
        public Task FetchAndActivateAsync() => Task.CompletedTask;
        public string GetString(string key, string defaultValue) => defaultValue;
        public int GetInt(string key, int defaultValue) => defaultValue;
        public float GetFloat(string key, float defaultValue) => defaultValue;
        public bool GetBool(string key, bool defaultValue) => defaultValue;
    }
}
