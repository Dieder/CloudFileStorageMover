## Why

When selecting "Desktop" as a source location, the file selection page shows only raw Desktop contents (files/folders physically on the desktop). Users expect to see well-known system folders — Downloads, Documents, Pictures, Videos, Music — as root-level entries they can browse into, identical to what Windows File Explorer shows as "quick access" folders.

## What Changes

- When the Desktop location is selected, inject well-known system folders (Downloads, Documents, Pictures, Videos, Music) as virtual entries at the top of the file list
- Clicking a system folder navigates directly to that folder's real path (e.g., `C:\Users\<user>\Downloads`)
- Actual Desktop contents are shown below the system folders
- The behavior applies only when browsing the "local-desktop" location; other locations (Downloads, Documents, Pictures) remain unchanged

## Capabilities

### New Capabilities
- `system-folder-browsing`: Inject well-known Windows system folders as navigable entries when browsing the Desktop location in the file selection screen

### Modified Capabilities
- (none)

## Impact

- `ViewModels/FileSelectionViewModel.cs` — Detect desktop location and inject system folder entries
- `Pages/FileSelectionPage.xaml.cs` — Handle navigation into system folder entries
- User-facing: Desktop browsing shows Downloads, Documents, Pictures, Videos, Music as root entries
