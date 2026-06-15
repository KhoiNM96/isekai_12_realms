# Remote Config Setup

Firebase Remote Config is optional. The game uses local defaults while `USE_FIREBASE_REMOTE_CONFIG` is not defined.

## Steps
- Install Firebase Remote Config only after Firebase base packages are configured.
- Add `USE_FIREBASE_REMOTE_CONFIG` to Player Settings Scripting Define Symbols after the package imports cleanly.
- Keep game startup non-blocking. Remote config fetch failures must use local defaults.

## Recommended Keys
- `iap_enabled`
- `remote_content_enabled`
- `daily_shop_refresh_hour`
- `max_level_cap`
- `gold_reward_multiplier`
- `exp_reward_multiplier`
- `current_content_version`
- `minimum_app_version`
- `maintenance_message`
