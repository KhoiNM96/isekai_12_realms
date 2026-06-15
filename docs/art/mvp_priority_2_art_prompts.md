# MVP Priority 2 Art Prompts

Unity AI image generation is not configured in this workspace. Generate original PNG assets externally using the exact filenames and folders from the task, then copy them into `Assets/_Game/Art/Generated/`.

Global requirements:
- Original cute chibi isekai fantasy art.
- No copied game assets, no watermark, no text inside PNGs.
- Transparent assets must have transparent backgrounds.
- Full backgrounds must be opaque.
- Filename format must be `object_widthxheight.png`.
- Create matching JSON metadata in `Assets/_Game/Art/Generated/Meta/`.

Prompt template:

```text
Create an original PNG game asset for a mobile portrait isekai RPG match-3 game.
Asset ID: {asset_id}
Size: {width}x{height}
Background: {transparent/full scene}
Style: cute chibi isekai fantasy, bright colors, clean readable shape, soft outline, mobile UI friendly, non-violent, no blood, no gore.
Description: {description}
Do not include text, watermark, copyrighted characters, historical real persons, or copied game assets.
Output filename: {asset_id}_{width}x{height}.png
```

Use the Priority 2 list in `docs/asset_manifest.md` plus the Realm 01-03 task list as the required batch.
