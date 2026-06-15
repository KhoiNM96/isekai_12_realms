using System.Threading.Tasks;
using Isekai12Realms.Data;
using Isekai12Realms.Services;

namespace Isekai12Realms.Auth
{
    public class MockAuthService : IAuthService
    {
        private readonly ISaveService saveService;
        private AuthUserData currentUser;

        public MockAuthService(ISaveService save)
        {
            saveService = save;
        }

        public bool IsAvailable => true;
        public bool IsSignedIn => currentUser != null;
        public AuthUserData CurrentUser => currentUser;

        public Task<AuthUserData> SignInAnonymousAsync()
        {
            PlayerSaveData save = saveService?.CurrentSave;
            string uid = !string.IsNullOrEmpty(save?.localGuestId) ? save.localGuestId : save?.deviceId;
            currentUser = new AuthUserData { uid = uid, displayName = save?.playerName ?? "Guest Hero", isAnonymous = true, providerType = AuthProviderType.LocalOnly };
            return Task.FromResult(currentUser);
        }

        public Task<AuthUserData> SignInGoogleAsync()
        {
            return SignInAnonymousAsync();
        }

        public Task SignOutAsync()
        {
            currentUser = null;
            return Task.CompletedTask;
        }
    }
}
