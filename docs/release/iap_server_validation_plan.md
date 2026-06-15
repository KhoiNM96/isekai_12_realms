# IAP Server Validation Plan

Client-only receipt validation is weaker because the client can be modified, delayed, or replayed. For production monetization, move validation to a trusted backend.

## Recommended Endpoint
- `verifyGooglePlayPurchase`

## Expected Request
```json
{
  "uid": "user_uid",
  "productId": "gems_tiny",
  "purchaseToken": "google_play_token",
  "transactionId": "transaction_id"
}
```

## Expected Response
```json
{
  "valid": true,
  "alreadyGranted": false,
  "totalGranted": 120
}
```

## Firestore Purchase Ledger Path
- `/users/{uid}/purchases/{transactionId}`

## Google Play Developer API Checklist
- Create a service account.
- Grant access to the Google Play Developer API.
- Enable the API for the publishing project.
- Link the app to Google Play Console.
- Verify consumable purchase tokens server-side.

## Service Account Permissions Checklist
- Read purchase state.
- Verify order tokens.
- Access the application purchase API.

## Grant-Once Rule
- Grant Soul Gems only once per `transactionId`.
- Persist the granted transaction in the local ledger immediately.
- Sync the ledger to Firestore after grant.
