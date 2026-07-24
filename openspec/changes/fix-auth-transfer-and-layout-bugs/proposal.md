## Why

Testing revealed 4 blocking bugs that crash or hang the application:

1. **Google refresh token fails** — Missing `client_secret` in refresh request causes 400 error; client_id never loaded from appsettings
2. **Desktop transfer crashes** — `Shell.Current.GoToAsync("//transfer")` throws because no Shell route exists for transfer page; file operations lack proper async/exception handling
3. **Meta login hangs** — Manual token prompt gives no success/failure feedback; SetTokens doesn't store refresh token, so IsSignedIn breaks on next launch
4. **Folder panel left of source selector** — Folder panel becomes visible when a source auto-signs in, showing it to the left of the source selection UI before user picks a source

Without fixes, the app is unusable for core workflows (cloud auth, local transfers, navigation).

## What Changes

- **Google Drive auth**: Add client_secret to refresh token request; load credentials from AppSettingsService into the auth service
- **Desktop transfer**: Fix navigation to use code-behind content switching instead of Shell routing; add async file operations with IProgress, proper exception handling, and cancellation support
- **Meta auth**: Add IsSignedIn check after SetTokens; store refresh token; show feedback on login result
- **Folder panel layout**: Gate IsFolderPanelVisible behind explicit source selection + folder load; don't auto-show on sign-in

## Capabilities

### Modified Capabilities
- `google-drive-sign-in`: Fix refresh token flow with proper credentials and secret
- `desktop-transfer-execution`: Fix navigation and add async/exception-safe file operations
- `meta-sign-in`: Fix login feedback and token persistence
- `folder-panel-layout`: Fix visibility gating

## Impact

- `Services/GoogleDriveSignInService.cs` — Add client_secret to refresh; load credentials
- `Services/DesktopStorageProvider.cs` — Replace sync file ops with async+progress+exception handling
- `Services/DownloadManager.cs` — Add IProgress wiring; safe stream cleanup
- `ViewModels/FileSelectionViewModel.cs` — Fix navigation to use content switching
- `ViewModels/SourceSelectionViewModel.cs` — Fix Meta login feedback; add Google Drive OAuth flow
- `MainPage.xaml.cs` — Expose content switching method
- `ViewModels/MainPageViewModel.cs` — Fix folder panel visibility gating
