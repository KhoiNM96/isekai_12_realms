# Production IAP + Firebase Setup

## Firebase
- Create a Firebase project.
- Add the Android app with the final package name.
- Download `google-services.json`.
- Put `google-services.json` in `Assets/`.
- Import Firebase Unity SDK Auth and Firestore packages.
- Enable Anonymous Auth.
- Enable Google provider if used.
- Deploy Firestore security rules.
- Add `USE_FIREBASE` scripting define.
- Test sign-in on an Android device.

## Unity IAP
- Install the Unity IAP package.
- Add `USE_UNITY_IAP` scripting define.
- Create Google Play Console in-app products:
  - `gems_tiny`
  - `gems_small`
  - `gems_medium`
  - `gems_large`
  - `gems_mega`
- Set products as consumable.
- Activate products.
- Add a license tester account.
- Upload an internal testing build.
- Test purchase with the tester.
- Verify Soul Gem grant once.
- Verify restart keeps gems.
- Verify duplicate transaction is ignored.
- Verify cloud ledger sync.

## Production Safety
- Run `Tools/Isekai 12 Realms/Production/Validate Production Build`.
- No debug UI.
- No mock purchase.
- No cheat buttons.
- No mock cloud.
- Version code incremented.
- AAB built with ARM64.
- Package name final.
