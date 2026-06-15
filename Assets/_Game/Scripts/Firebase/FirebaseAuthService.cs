using System.Threading.Tasks;
using Isekai12Realms.Auth;
using Isekai12Realms.Services;
using Isekai12Realms.UI;

#if USE_FIREBASE
using Firebase;
using Firebase.Auth;
#endif

namespace Isekai12Realms.FirebaseIntegration
{
    // To enable real Firebase: import Firebase Unity SDK Auth/Firestore, add google-services.json,
    // define USE_FIREBASE in Player Settings Scripting Define Symbols, and configure Firebase Console.
    public class FirebaseAuthService : IAuthService
    {
        private readonly ISaveService saveService;
        private readonly ToastService toastService;
        private AuthUserData currentUser;
#if USE_FIREBASE
        private FirebaseAuth auth;
#endif

        public FirebaseAuthService(ISaveService save, ToastService toast)
        {
            saveService = save;
            toastService = toast;
        }

#if USE_FIREBASE
        public bool IsAvailable => auth != null;
#else
        public bool IsAvailable => false;
#endif
        public bool IsSignedIn => currentUser != null;
        public AuthUserData CurrentUser => currentUser;
        public bool IsGoogleSignInConfigured => false;

        public async Task<AuthUserData> SignInAnonymousAsync()
        {
#if USE_FIREBASE
            try
            {
                DependencyStatus status = await FirebaseApp.CheckAndFixDependenciesAsync();
                if (status != DependencyStatus.Available) return null;
                auth = FirebaseAuth.DefaultInstance;
                FirebaseUser user = auth.CurrentUser ?? (await auth.SignInAnonymouslyAsync()).User;
                currentUser = new AuthUserData { uid = user.UserId, displayName = user.DisplayName, email = user.Email, isAnonymous = user.IsAnonymous, providerType = AuthProviderType.FirebaseAnonymous };
                if (saveService.CurrentSave != null)
                {
                    saveService.CurrentSave.firebaseUid = currentUser.uid;
                    saveService.CurrentSave.authProvider = AuthProviderType.FirebaseAnonymous.ToString();
                    saveService.SaveNow();
                }
                return currentUser;
            }
            catch (System.Exception e)
            {
                UnityEngine.Debug.LogWarning("[FirebaseAuth] Anonymous sign-in failed: " + e.Message);
                return null;
            }
#else
            await Task.CompletedTask;
            return null;
#endif
        }

        public Task<AuthUserData> SignInGoogleAsync()
        {
#if USE_FIREBASE
            if (!IsGoogleSignInConfigured)
            {
                return Task.FromResult<AuthUserData>(null);
            }
#endif
            toastService?.ShowToast("Google Sign-In is not configured yet.");
            return Task.FromResult<AuthUserData>(null);
        }

        public Task SignOutAsync()
        {
#if USE_FIREBASE
            auth?.SignOut();
#endif
            currentUser = null;
            return Task.CompletedTask;
        }
    }
}
