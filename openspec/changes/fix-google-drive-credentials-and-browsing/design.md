## Context

The credentials config page has two Entry fields for each provider: Field1 (Client ID / API Key) and Field2 (Client Secret / API Secret). Both use `IsPassword="True"`, hiding the Client ID value. Users cannot verify their configured Client ID without clearing and re-entering it.

Separately, the `GoogleDriveStorageProvider` authenticates via OAuth2 by loading a `client_secrets.json` file from disk at runtime. However, users enter their Google OAuth2 Client ID and Secret via the credentials config UI, which saves them to `ICredentialsService` (SecureStorage). The provider never reads these saved credentials, so authentication always fails, and Google Drive shows no contents.

The `FileMoverService.GetProviderForLocation` method also has a hardcoded fallback to `"local"` for unmapped location IDs, which breaks file listing for Google Drive folders.

## Goals / Non-Goals

**Goals:**
- Make the Client ID visible in the credentials config page (remove password masking)
- Update `GoogleDriveStorageProvider` to use saved ClientId/ClientSecret from `ICredentialsService` instead of requiring `client_secrets.json`
- Fix `FileMoverService.GetProviderForLocation` to correctly resolve provider for cloud locations
- Refresh provider configuration status in `SourceSelectionViewModel` after returning from credentials config

**Non-Goals:**
- Not changing the Client Secret field (stays masked)
- Not implementing full OAuth2 token refresh (same as before)
- Not modifying the other provider implementations (Windows, Prime)

## Decisions

| Decision | Rationale |
|---|---|
| **Remove `IsPassword` on Field1 only** | Client ID is not sensitive (it's an identifier, not a secret). Client Secret stays masked. |
| **Construct `ClientSecrets` in code from saved credentials** | Avoids needing a `client_secrets.json` file on disk. `GoogleClientSecrets` can be built from `ClientSecrets` object with ClientId + ClientSecret. |
| **Inject `ICredentialsService` into `GoogleDriveStorageProvider`** | Cleanest way to access saved credentials. The provider already takes other DI services. |
| **Update `FileMoverService.GetProviderForLocation`** | Need to match `locationId` prefix patterns (e.g., Google Drive folder IDs) to the correct provider type instead of defaulting to `"local"`. |

## Risks / Trade-offs

- **[OAuth2 flow change]** Moving from file-based to in-memory credential construction may behave differently with `GoogleWebAuthorizationBroker`. Mitigation: test with a real Google Drive OAuth2 setup.
- **[Backward compatibility]** Existing `client_secrets.json` files on disk become unused. Users who manually placed the file will need to enter credentials via the UI instead. Mitigation: the old file path is no longer required; credentials always come from SecureStorage.
