## 1. Config & Setup

- [x] 1.1 Create `config/providers/` directory and JSON config files for Google Drive, Windows, and Prime
- [x] 1.2 Add `Google.Apis.Drive.v3` NuGet package to `FileMover.UI.csproj`
- [x] 1.3 Create `Models/ProviderConfig.cs` typed model class for config deserialization
- [x] 1.4 Create `Services/ProviderConfigLoader.cs` to read and deserialize config files

## 2. IStorageProviderService Interface

- [x] 2.1 Create `Services/IStorageProviderService.cs` with async methods: `ListFoldersAsync`, `ListFilesAsync`, `CreateFileAsync`, `UploadFileAsync`, `RenameAsync`, `DeleteAsync`
- [x] 2.2 Register interface in DI via `MauiProgram.cs`

## 3. Windows Storage Provider

- [x] 3.1 Create `Services/WindowsStorageProvider.cs` implementing `IStorageProviderService` using `System.IO`
- [x] 3.2 Register `WindowsStorageProvider` in DI via `MauiProgram.cs`

## 4. Google Drive Provider

- [x] 4.1 Create `Services/GoogleDriveStorageProvider.cs` with OAuth2 authentication flow
- [x] 4.2 Implement folder/file listing using Drive v3 API
- [x] 4.3 Implement file create, upload, rename, delete using Drive v3 API
- [x] 4.4 Register `GoogleDriveStorageProvider` in DI via `MauiProgram.cs`

## 5. Prime Storage Provider

- [x] 5.1 Create `Services/PrimeStorageProvider.cs` implementing `IStorageProviderService`
- [x] 5.2 Register `PrimeStorageProvider` in DI via `MauiProgram.cs`

## 6. Wire Into FileMoverService

- [x] 6.1 Update `FileMoverService` to accept `IEnumerable<IStorageProviderService>` via constructor injection
- [x] 6.2 Add provider lookup logic to select the correct implementation based on provider type
- [x] 6.3 Delegate file operations (list, create, upload, rename, delete) to the selected provider
