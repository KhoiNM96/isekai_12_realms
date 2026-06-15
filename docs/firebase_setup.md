# Firebase Setup Guide

Firebase is optional. The game compiles and plays local-only when the Firebase SDK, config files, or `USE_FIREBASE` define are missing.

## Required Firebase Modules
- Authentication
- Cloud Firestore

## Enable Auth Providers
- Enable Anonymous Auth in Firebase Console.
- Enable Google Sign-In later when the Google Sign-In package and platform credentials are configured.

## Unity Setup
1. Import Firebase Unity SDK Auth and Firestore packages.
2. Add the Android app package name in Firebase Console.
3. Download `google-services.json`.
4. Put `google-services.json` under `Assets/`.
5. Add `USE_FIREBASE` to Player Settings Scripting Define Symbols.
6. Configure your Firebase project and test Anonymous Auth before Google Sign-In.

## Firestore Paths
- `/users/{uid}/profile/main`
- `/users/{uid}/saves/default`
- `/users/{uid}/purchases/{transactionId}`

## Security Rules Draft
```js
service cloud.firestore {
  match /databases/{database}/documents {
    function signedIn() {
      return request.auth != null;
    }

    function isOwner(uid) {
      return signedIn() && request.auth.uid == uid;
    }

    match /users/{uid} {
      allow read, write: if isOwner(uid);

      match /{document=**} {
        allow read, write: if isOwner(uid);
      }
    }
  }
}
```

## Notes
- IAP remains a placeholder and must not sell direct equipment or power items.
- Cloud purchase records are backup/audit data only. Local save remains the source of truth for placeholder grants.
- If Firebase fails at runtime, keep playing with local save.
