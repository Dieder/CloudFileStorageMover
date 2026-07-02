## Context

The app currently has a single `IFileMoverService` interface focused on transfer operations and a mock implementation. There are no real storage provider backends. For Google Drive integration, the `Google.Apis.Drive.v3` SDK requires OAuth2 authentication via `client_secrets.json`. The user also wants local file system access and a Prime provider. API keys and secrets must be stored in config files, not hardcoded.

## Goals / Non-Goals

**Goals:**
- Define `IStorageProviderService` interface with async methods for: list folders, list files, create, upload, rename, delete
- Implement `WindowsStorageProvider` using `System.IO`
- Implement `GoogleDriveStorageProvider` using `Google.Apis.Drive.v3` with OAuth2
- Implement `PrimeStorageProvider`
- Store credentials in `config/providers/<provider>.json` files
- Wire providers into `IFileMoverService` so the UI can use them

**Non-Goals:**
- Not modifying the existing `IFileMoverService` interface (it stays for transfer orchestration)
- Not implementing real OAuth2 token refresh/management beyond basic flow
- Not adding UI for provider configuration (config files are edited manually)

## Decisions

| Decision | Rationale |
|---|---|
| **New `IStorageProviderService` interface** instead of extending `IFileMoverService` | Separation of concerns: file operations vs transfer orchestration. Keeps each interface focused. |
| **Config files in `config/providers/<provider>.json`** | Standard JSON format, easy to edit, gitignored for secrets. Each provider has its own file. |
| **Google Drive uses OAuth2 with `client_secrets.json`** | Follows Google's recommended auth flow. The file is loaded at runtime and the credential is stored in-memory. |
| **Windows provider uses `System.IO` directly** | No external dependencies needed; async wrappers around `Directory.Enumerate*` and `FileStream`. |
| **Provider instances registered via DI** | `MauiProgram.cs` registers each provider, `FileMoverService` selects the right one based on `StorageProvider.Type`. |

## Risks / Trade-offs

- **[Google API quota]** Google Drive API has usage quotas. Mitigation: document that users need to enable the Drive API in Google Cloud Console.
- **[OAuth2 token expiry]** The basic OAuth2 flow doesn't handle token refresh. Mitigation: document that users may need to re-authenticate periodically.
- **[Config file security]** Storing secrets in JSON files is basic. Mitigation: add `config/providers/` to `.gitignore` and document the security considerations.
