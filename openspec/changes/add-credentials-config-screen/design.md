## Context

The app has `StorageProvider` model with an `IsConfigured` property, but no UI to set credentials. The `MockFileMoverService` returns hardcoded providers. Credentials for Google Drive (OAuth2 client secrets) and Prime (API key/secret) must currently be edited in config files manually.

## Goals / Non-Goals

**Goals:**
- Provide a credentials configuration page accessible via a toolbar at the top of key screens
- Securely store provider credentials using platform APIs (`SecureStorage` on mobile, encrypted storage on desktop)
- Show per-provider configured/unconfigured status in source selection
- Allow "Configure" button on unconfigured providers that navigates to the config page

**Non-Goals:**
- Not implementing actual authentication flow (OAuth2 token exchange, refresh) — just storing the raw credentials
- Not adding credential encryption beyond what `SecureStorage` provides
- Not modifying any existing provider service implementations

## Decisions

| Decision | Rationale |
|---|---|
| **Use `SecureStorage` as primary credential store** | Cross-platform MAUI API; works on all targets with platform-native secure storage |
| **Fallback to encrypted JSON for Windows desktop** | `SecureStorage` on Windows uses `DataProtectionProvider` which may have issues in some cases; encrypted JSON is a reliable fallback |
| **Credentials are stored per-provider keyed by provider ID** | Simple dictionary lookup; each provider has its own config credentials (Google: ClientId, ClientSecret; Prime: ApiKey, ApiSecret) |
| **Single `CredentialsConfigPage` with provider selection** | One page that shows a list of configurable providers and expands to show fields for the selected provider — avoids N separate pages |
| **Shell ToolbarItem on key pages** (home, source/destination selection, file selection) | Visible from all major screens; MAUI Shell already provides the toolbar infrastructure; gear icon follows platform convention for settings |

## Risks / Trade-offs

- **[SecureStorage data loss on uninstall]** Uninstalling the app wipes SecureStorage. Mitigation: document that users should back up credentials.
- **[No OAuth token exchange]** This stores raw credentials, not tokens. The actual OAuth flow is a separate concern for each provider implementation. Mitigation: clear naming — "credentials" not "authentication".
- **[Prime credentials format unknown]** Without spec from Prime, we assume key/secret pair. Mitigation: use flexible `Dictionary<string, string>` for credential fields so new fields can be added without code changes.
