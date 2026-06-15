# Unity IAP Setup Guide

Unity IAP is optional. The project compiles without the Unity IAP package until `USE_UNITY_IAP` is defined.

## Setup Steps
- Install the Unity IAP package.
- Enable Unity Gaming Services if required by your project setup.
- Create these consumable products in Unity/Google Play Console:
- `gems_tiny`
- `gems_small`
- `gems_medium`
- `gems_large`
- `gems_mega`
- Use the same product IDs in Google Play Console and `IAPProductDefinition.platformProductId`.
- Define `USE_UNITY_IAP` after package setup.
- Test only with a Google Play license tester account.
- Never test real cards on production without internal testing.

## Rules
- IAP only grants Soul Gem currency.
- Do not sell equipment, items, level boosts, or stage unlocks through IAP.
- All products are consumable.
- The local purchase ledger prevents duplicate grants by `transactionId`.
- Receipt storage keeps only `receiptHash`, not raw receipt.
- For production monetization, see `docs/release/production_iap_firebase_setup.md` and `docs/release/iap_server_validation_plan.md`.
