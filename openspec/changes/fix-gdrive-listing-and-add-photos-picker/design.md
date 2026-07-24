## Context

The `GoogleDriveStorageProvider` currently constructs Google Drive API queries by manipulating path strings (e.g., joining folder names with `' or name='`), which produces invalid queries. The Drive v3 API expects folder traversal via `'{folderId}' in parents` syntax. The provider also has a `GooglePhotosPickerService` in `Custom.Google.Services` that wraps the `photospicker.googleapis.com` API but is not wired into the app UI.

The left menu bar (`LeftMenuBar`) has Source, Transfer, Config, and Quit buttons. A new Google Photos Picker button is needed for quick photo import.

## Goals / Non-Goals

**Goals:**
- Fix Google Drive folder/file listing to work after OAuth sign-in by using proper folder ID-based queries
- Add a Google Photos Picker button to the left shell menu that opens the picker, lets users select photos, and downloads them to the configured images folder
- Reuse existing `GooglePhotosPickerService` and `GooglePhotosSignInService` for the picker flow

**Non-Goals:**
- Changing the `IStorageProviderService` interface
- Modifying the Google Photos *source provider* (the existing `GooglePhotosStorageProvider` for browsing albums)
- Adding new OAuth flows — reusing existing Google OAuth with `photoslibrary.readonly` + `photospicker` scopes

## Decisions

### 1. Google Drive listing fix: Use folder ID-based traversal

**Decision:** Rewrite `ListFilesInternalAsync` to accept a `folderId` parameter and query `'{folderId}' in parents`. `ListFoldersAsync` and `ListFilesAsync` pass the folder ID directly (the `FullPath` of a `FileItem` is already the Drive file ID). `GetCommonDirectoriesAsync` returns a single entry with the literal `"root"` ID which Drive v3 resolves to the user's root folder.

**Alternatives considered:**
- Fixing the path string parsing — rejected because path-based traversal requires sequential API calls per path segment, adding latency and complexity with no benefit over ID-based queries.

### 2. Google Photos Picker: Standalone flow, not a storage provider

**Decision:** Implement the picker as a dedicated flow triggered from the left menu, not as an `IStorageProviderService`. The flow: create session → open `https://photos.google.com/create-session` in browser → poll for selections → download to local folder. This avoids conflating the picker (user selects specific photos) with the source browser (user navigates albums).

**Alternatives considered:**
- Making it a new `StorageProviderType` — rejected because the picker has a fundamentally different UX (browser-based selection, no folder navigation) and doesn't fit the `IStorageProviderService` contract.

### 3. Download target: Use `DestinationSettingsService` for path resolution

**Decision:** Download selected photos to `Environment.SpecialFolder.MyPictures` (or the configured "Afbeeldingen" path from settings). Use `DestinationSettingsService` to resolve the target, falling back to the platform pictures folder.

### 4. OAuth scope for picker

**Decision:** Request `https://www.googleapis.com/auth/photoslibrary.readonly` + `https://www.googleapis.com/auth/photospicker.readonly` during the picker flow. Reuse the existing `ExchangeGoogleCodeAsync` pattern from `SourceSelectionViewModel` with the combined scope.

## Risks / Trade-offs

- **Polling latency** → The picker session polling may take a few seconds after the user selects photos in the browser. Show a loading indicator during this phase.
- **Token scope mismatch** → If the user already signed in to Google Drive with only `drive.readdownscope`, the picker needs `photospicker.readonly`. Mitigation: the picker flow performs its own OAuth exchange with the correct scope if the existing token doesn't cover it.
- **Browser session dependency** → The picker requires the user to complete selection in a browser tab. If the browser is closed without selecting, the flow times out gracefully after a configurable period.
