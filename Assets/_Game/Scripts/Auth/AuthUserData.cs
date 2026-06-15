using System;

namespace Isekai12Realms.Auth
{
    [Serializable]
    public class AuthUserData
    {
        public string uid;
        public string displayName;
        public string email;
        public bool isAnonymous;
        public AuthProviderType providerType;
    }
}
