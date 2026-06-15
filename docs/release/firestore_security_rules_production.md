# Firestore Security Rules Production

```js
rules_version = '2';
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

## Deployment Checklist
- Enable Firebase Authentication.
- Enable Anonymous provider.
- Enable Google provider if used.
- Enable Firestore.
- Deploy security rules.
- Add `google-services.json` to `Assets/`.
- Define `USE_FIREBASE`.
- Test on an Android device.
