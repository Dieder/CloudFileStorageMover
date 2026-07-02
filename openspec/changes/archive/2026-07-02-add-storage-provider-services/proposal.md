## Why

The app currently uses a single `IFileMoverService` interface and a mock service. Real storage providers (Google Drive, local file system, etc.) need concrete implementations with proper async file operations and API authentication.

## What Changes

- Add a new `IStorageProviderService` interface with async folder/file operations: list folders, list files, create, upload, rename, delete
- Implement `WindowsStorageProvider` using `System.IO` for local file system access
- Implement `GoogleDriveStorageProvider` using `Google.Apis.Drive.v3` NuGet package with OAuth2 authentication
- Implement `PrimeStorageProvider` for the third provider
- Store API keys and credentials in a JSON config file per provider
- Update the existing `IFileMoverService` to delegate to provider-specific implementations

## Capabilities

### New Capabilities
- `storage-provider-interface`: Common async interface for folder/file operations across all providers
- `windows-storage-provider`: Local file system implementation using System.IO
- `google-drive-provider`: Google Drive implementation using Google.Apis.Drive.v3
- `prime-storage-provider`: Provider implementation for Prime
- `provider-config`: Configuration file management for API keys and credentials per provider

### Modified Capabilities
- `file-mover-ui`: The existing spec needs a requirement for provider-specific storage backends

## Impact

- `Services/IStorageProviderService.cs` — new interface
- `Services/WindowsStorageProvider.cs` — new local file system provider
- `Services/GoogleDriveStorageProvider.cs` — new Google Drive provider
- `Services/PrimeStorageProvider.cs` — new Prime provider
- `Services/MockFileMoverService.cs` — updated to use provider pattern or kept as fallback
- `FileMover.UI.csproj` — add `Google.Apis.Drive.v3` NuGet package
- `config/providers/` — new config directory with per-provider JSON config files
- `Models/` — potential new config model classes
