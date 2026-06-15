using System.Threading.Tasks;

namespace Isekai12Realms.Auth
{
    public interface IAuthService
    {
        bool IsAvailable { get; }
        bool IsSignedIn { get; }
        AuthUserData CurrentUser { get; }
        Task<AuthUserData> SignInAnonymousAsync();
        Task<AuthUserData> SignInGoogleAsync();
        Task SignOutAsync();
    }
}
