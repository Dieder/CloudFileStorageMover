## 1. Fix Google Drive Refresh Token

- [ ] 1.1 Add `client_secret` parameter to `RefreshTokenAsync()` POST body in `GoogleDriveSignInService`
- [ ] 1.2 Load Google OAuth credentials (client_id, secret) from `AppSettingsService` into `GoogleDriveSignInService` settings store
- [ ] 1.3 Add proper Google Drive OAuth flow in `SourceSelectionViewModel.AuthenticateCurrentProviderAsync()` using `WebAuthenticator`

## 2. Fix Desktop Transfer Crash

- [ ] 2.1 Create `AsyncFileOperations` helper class with `CopyAsync` and `MoveAsync` supporting `IProgress<long>` and `CancellationToken`
- [ ] 2.2 Update `DesktopStorageProvider.DownloadAsync` and `UploadAsync` to use `AsyncFileOperations` with proper async/exception handling
- [ ] 2.3 Expose `SwitchToPage` on `MainPage` for ViewModel-triggered content switching
- [ ] 2.4 Fix `FileSelectionViewModel.TransferSelectedAsync()` to use `MainPage.SwitchToPage` instead of `Shell.Current.GoToAsync`

## 3. Fix Meta Login Hang

- [ ] 3.1 Show success/failure alert after Meta token entry
- [ ] 3.2 Store refresh token in `MetaSignInService.SetTokens` and fix `IsSignedIn` to persist correctly

## 4. Fix Folder Panel Layout

- [ ] 4.1 Gate `IsFolderPanelVisible` to only become true when user explicitly selects a source AND folders are loaded
- [ ] 4.2 Don't auto-show folder panel on `LoadFoldersAsync` if user hasn't explicitly selected the source

## 5. Build and Verify

- [ ] 5.1 Build project with 0 errors
- [ ] 5.2 Verify Google Drive sign-in and refresh work
- [ ] 5.3 Verify desktop transfer works without crash
- [ ] 5.4 Verify Meta login shows feedback
- [ ] 5.5 Verify folder panel only shows after source selection
