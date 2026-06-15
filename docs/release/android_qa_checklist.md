# Android QA Checklist

## Fresh Install Test
- Install APK on a clean device.
- Launch from BootScene into GameScene.
- Start a new hero and reach Main Town.

## Returning Player Test
- Close and relaunch the app.
- Confirm local save loads with the same hero, gold, level, and progress.

## Battle Test
- Enter Stage 1-1.
- Match tokens, use skills, win, lose, and leave battle from pause.

## Reward/Save Test
- Win a battle.
- Confirm EXP, gold, drops, and stage clear are saved after relaunch.

## Inventory/Equipment Test
- Add or earn equipment.
- Equip it and confirm stats update.

## Skill Upgrade Test
- Gain enough currency/materials.
- Upgrade a skill and confirm costs are consumed.

## Quest/Tutorial Test
- Start a fresh save.
- Confirm tutorial appears and quest progress updates.

## Shop Soft Currency Test
- Buy a soft-currency item.
- Confirm currency is deducted and item/effect is granted.

## IAP Mock Test
- In Editor/development QA flow, use mock IAP only.
- Confirm Soul Gems are granted and purchase records are created.

## Cloud Save Mock Test
- Use guest/mock cloud save flow.
- Confirm failures do not block offline play.

## Offline Mode Test
- Launch and play with network disabled.
- Confirm save, battle, shop, inventory, skills, and quests still work.

## App Pause/Resume Test
- Pause during town and battle.
- Resume and confirm local save is intact and battle remains readable.

## Low Memory Test
- Test on a low RAM device.
- Confirm frame rate fallback and reduced VFX remain readable.

## Different Aspect Ratio Test
- Test 720x1280, 1080x1920, and 1080x2400.
- Confirm safe area and bottom navigation are usable.

## No Internet Test
- Enable airplane mode.
- Confirm no blocking network errors appear.

## Performance Test
- Play 10 battles.
- Watch for frame drops, overheating, excessive VFX spam, and memory growth.

## Build Validation Checklist
- Run `Tools/Isekai 12 Realms/Build/Apply Android Build Settings`.
- Run `Tools/Isekai 12 Realms/Build/Run Build Validator`.
- Run sprite import validation.
- Run save migration test.
- Run manual smoke checks.
