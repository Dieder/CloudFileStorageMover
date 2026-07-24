## 1. Fix Google Drive Folder/File Listing

- [x] 1.1 Rewrite `GoogleDriveStorageProvider.ListFilesInternalAsync` to accept a `folderId` parameter and query `'{folderId}' in parents` with mimeType filter for folders vs files
- [x] 1.2 Update `GoogleDriveStorageProvider.ListFoldersAsync` to pass the folder ID directly from `path` parameter to `ListFilesInternalAsync`
- [x] 1.3 Update `GoogleDriveStorageProvider.ListFilesAsync` to pass the folder ID directly and exclude folders from results
- [x] 1.4 Update `GoogleDriveStorageProvider.GetCommonDirectoriesAsync` to return `"root|Google Drive"` as the single root entry
- [x] 1.5 Remove the broken `GetFolderIdFromPathAsync` method and path string manipulation logic
- [x] 1.6 Test Google Drive sign-in → folder listing → file listing → folder navigation end-to-end

## 2. Add Google Photos Picker Button to Shell Menu

- [x] 2.1 Add `GooglePhotosPicker` value to `AppMenuItem` enum in `LeftMenuBar.xaml.cs`
- [x] 2.2 Add Google Photos Picker icon button to `LeftMenuBar.xaml` between Source and Transfer buttons
- [x] 2.3 Add click handler and event in `LeftMenuBar.xaml.cs` for the picker button
- [x] 2.4 Add `PickerRequested` event handler wiring in `MainPage.xaml.cs`

## 3. Implement Google Photos Picker Flow

- [x] 3.1 Create `GooglePhotosPickerService` wrapper in `CloudStorageMover.Core/Services` that uses `Custom.Google.Services.Services.GooglePhotosPickerService` for session creation, polling, and download
- [x] 3.2 Register `GooglePhotosPickerService` in `MauiProgram.cs` DI container
- [x] 3.3 Implement picker OAuth flow with `photospicker.readonly` scope using existing `ExchangeGoogleCodeAsync` pattern
- [x] 3.4 Implement session creation → browser open → polling loop with 120s timeout
- [x] 3.5 Implement download of selected photos to `Environment.SpecialFolder.MyPictures` (or configured images path)
- [x] 3.6 Add progress feedback (loading indicator, file count) during download
- [x] 3.7 Handle errors: token refresh, empty selection, network failure, timeout

## 4. Integration & Polish

- [x] 4.1 Wire picker button click to trigger the full picker flow from `MainPage`
- [x] 4.2 Show loading state in left menu or main area during picker flow
- [x] 4.3 Display success/failure summary after download completes
- [x] 4.4 Build and verify no compilation errors across all projects
