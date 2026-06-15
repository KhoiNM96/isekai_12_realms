# Android Build Steps

1. Generate placeholder PNGs.
2. Rebuild Asset Manifest.
3. Create prototype content.
4. Rebuild Content Database.
5. Validate content.
6. Apply Android Build Settings.
7. Run Build Validator.
8. Build APK for dev test.
9. Test on device.
10. Build AAB for internal testing.

## Unity Menus
- `Tools/Isekai 12 Realms/Assets/Fix Generated Sprite Import Settings`
- `Tools/Isekai 12 Realms/Rebuild Content Database`
- `Tools/Isekai 12 Realms/Validate Content`
- `Tools/Isekai 12 Realms/Build/Apply Android Build Settings`
- `Tools/Isekai 12 Realms/Build/Build Window`
- `Tools/Isekai 12 Realms/QA/Test Save Migration`
- `Tools/Isekai 12 Realms/QA/Run Manual Smoke Checks`
- `Tools/Isekai 12 Realms/Production/Clean Debug UI From Scenes`
- `Tools/Isekai 12 Realms/Production/Validate Production Build`
- `Tools/Isekai 12 Realms/Production/Prepare Production Build`

## Notes
- Firebase, IAP, and Addressables packages are optional.
- Core content must remain locally available.
- Do not enable real purchases outside an approved store test environment.
