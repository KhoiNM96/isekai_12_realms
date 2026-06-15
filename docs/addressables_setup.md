# Addressables Setup

Addressables are optional. The project compiles and plays without the package while `USE_ADDRESSABLES` is not defined.

## Steps
- Install the Unity Addressables package from Package Manager.
- Add `USE_ADDRESSABLES` to Player Settings Scripting Define Symbols only after the package imports cleanly.
- Create local and remote Addressables groups under `Assets/_Game/Addressables/Local/` and `Assets/_Game/Addressables/Remote/`.
- Mark asset addresses using the `assetId` from `GameAssetManifest` and content data.
- Build Addressables content before testing Addressables loads.
- Configure a remote load path later for optional realm, cosmetic, audio, or event packs.

## Fallback Rules
- Core content must always be included locally.
- Do not put core assets only in a remote group.
- Missing Addressables fall back to `GameAssetManifest.GetSprite(assetId)`.
- If remote catalogs fail or the player is offline, gameplay continues with local placeholder assets.
