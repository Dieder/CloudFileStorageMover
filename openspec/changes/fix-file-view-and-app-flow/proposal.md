## Why

The app currently starts with a HomePage that shows a "Start New Transfer" button, but the user needs to pick a mode (Server or Client) first. When browsing files, the filter buttons for photos, videos, and documents don't properly filter from the full file list — they filter from the already-filtered subset, making them behave incorrectly. Additionally, Google Drive and Prime sources show no content when selected.

## What Changes

- **BREAKING**: Replace HomePage as the app entry with a mode selection page that offers Server or Client choice
- In Server mode, the user picks a source (Google Drive, Prime, Desktop) then browses files with working filter buttons
- Fix file view filtering: store the full file list and always filter from it, so Image/Document/Video filters produce correct results
- Fix filter button visual toggle state across all filter modes
- Route Client mode to WiFi connection flow (QR scan / USB pairing)

## Capabilities

### New Capabilities
- `app-entry-mode-selection`: Server/Client mode selection as the app's initial screen

### Modified Capabilities
- `file-mover-ui`: Replace HomePage entry with mode selection; fix file view filter behavior

## Impact

- `Pages/ModeSelectionPage.xaml` — new Server/Client mode selection page
- `Pages/ModeSelectionPage.xaml.cs` — new code-behind with navigation logic
- `ViewModels/ModeSelectionViewModel.cs` — new ViewModel for mode selection
- `AppShell.xaml` — replace HomePage ShellContent with ModeSelectionPage
- `Pages/HomePage.xaml` — remove as app start page (keep as optional history view)
- `AppShell.xaml.cs` — register route for mode-selection
- `Pages/FileSelectionPage.xaml.cs` — fix `ApplyFilter` to filter from full file list; fix filter button state
- `ViewModels/FileSelectionViewModel.cs` — add `AllFiles` backing list for filter operations
- `MauiProgram.cs` — register new page and ViewModel in DI
