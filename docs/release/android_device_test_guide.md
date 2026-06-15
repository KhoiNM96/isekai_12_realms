# Android Device Test Guide

## Device Matrix
- 720x1280 phone.
- 1080x1920 phone.
- 1080x2400 phone.
- Low RAM device, ideally 2 GB RAM or lower.
- Android 8, 10, 12, and 14 if available.

## Required Scenarios
- Offline mode from first launch.
- Airplane mode while already in game.
- App pause/resume from Main Town.
- App pause/resume during battle.
- Rotate device and confirm the app remains portrait.
- Android back button on Title, Main Town, sub-screens, popups, and battle.
- Low battery/performance observation during repeated battles.

## Back Button Behavior
- Popup open: closes top popup.
- Sub-screen: returns to Main Town.
- Main Town: shows quit confirmation.
- Title: quits on device or does nothing in Editor.
- Battle: shows `Leave battle?` pause popup.
