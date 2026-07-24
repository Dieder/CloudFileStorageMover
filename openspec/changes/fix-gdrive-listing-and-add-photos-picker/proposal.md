## Why

After successfully signing in with Google, no files or folders appear in the file browser. The `GoogleDriveStorageProvider` constructs malformed API queries using path string manipulation instead of proper folder ID-based queries, causing the Drive v3 API to return empty results. Additionally, users need a quick-access way to import photos from Google Photos into their local images folder without navigating the full source selection flow.

## What Changes

- **Fix Google Drive folder/file listing**: Rewrite `ListFilesInternalAsync` to traverse the folder hierarchy using proper Drive v3 `'{folderId}' in parents` queries instead of broken name-based path joining. Fix `GetCommonDirectoriesAsync` to return a usable root reference.
- **Add Google Photos Picker button to shell menu**: Add a new icon button in the left menu bar that triggers the Google Photos Picker flow (create session → open picker in browser → poll for selections → download selected photos to the desktop "Afbeeldingen" folder as configured in settings).

## Capabilities

### New Capabilities
- `google-photos-picker`: Quick-access Google Photos Picker integration via the photospicker.googleapis.com API, triggered from the left shell menu, downloading selected photos to a configured local folder.

### Modified Capabilities
- `google-drive-provider`: Fix folder and file listing to use proper folder ID-based queries instead of broken path string construction.

## Impact

- `CloudStorageMover.Core/Services/GoogleDriveStorageProvider.cs` — rewrite of `ListFilesInternalAsync`, `ListFoldersAsync`, `ListFilesAsync`, and `GetCommonDirectoriesAsync`
- `CloudStorageMover.Controls/Controls/LeftMenuBar.xaml` + `.cs` — add Google Photos Picker button and `AppMenuItem.GooglePhotosPicker` enum value
- `CloudStorageMover.App/MainPage.xaml.cs` — wire up picker button handler
- `CloudStorageMover.App/ViewModels/MainPageViewModel.cs` — add navigation state for picker mode
- New ViewModel or service class for the Google Photos Picker flow (session creation, polling, download)
- `DestinationSettingsService` — used to resolve the target download folder (images/desktop path)
- No breaking changes to `IStorageProviderService` interface
