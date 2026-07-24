## Context

The app has 4 distinct bugs across auth, transfer, and UI layout that make core workflows fail.

## Goals / Non-Goals

**Goals:**
- Fix Google Drive token refresh by including client_secret in the POST
- Load Google OAuth credentials from AppSettingsService into GoogleDriveSignInService
- Fix desktop transfer navigation to not use Shell routing
- Add proper async file operations with IProgress, exception handling, and cancellation to DesktopStorageProvider
- Fix Meta login to show success/failure feedback and persist refresh token
- Fix folder panel to only show after user explicitly selects a source and loads folders
- Keep changes minimal and focused on the 4 bugs

**Non-Goals:**
- Refactoring auth services to use secure storage (future work)
- Adding new storage providers or features
- Changing the overall 5-zone layout structure

## Decisions

- **Decision**: Expose `SwitchToPage` method on MainPage for ViewModel-triggered navigation
  - **Rationale**: ViewModels shouldn't depend on Shell routing. Content switching through MainPage is the established pattern.
  - **Alternative**: Register Shell routes — would require restructuring navigation approach

- **Decision**: Add `AsyncFileOperations` helper with CopyAsync, MoveAsync supporting IProgress and CancellationToken
  - **Rationale**: The user provided this pattern. It encapsulates async file ops with proper error handling and progress reporting.
  - **Risk**: Additional abstraction layer — but keeps DesktopStorageProvider clean

- **Decision**: Load Google credentials into auth service's settings store at construction time
  - **Rationale**: Simple, minimal change. Avoids restructuring the auth architecture.
  - **Alternative**: Inject AppSettingsService into GoogleDriveSignInService — more coupling

- **Decision**: Show explicit alert for Meta login success/failure
  - **Rationale**: The user currently gets no feedback. An alert is the simplest fix.
  - **Alternative**: Status bar message — less visible

## Risks / Trade-offs

- [Google Drive secret in memory] → Secret stays in process memory; acceptable for desktop app
- [AsyncFileOperations new class] → Must be tested for edge cases (permissions, locked files, cross-volume)
- [Folder panel gating change] → May affect other flows that depend on IsFolderPanelVisible timing
