## 1. Mode Selection Page

- [x] 1.1 Create `Pages/ModeSelectionPage.xaml` with Server and Client option buttons
- [x] 1.2 Create `Pages/ModeSelectionPage.xaml.cs` with navigation handlers (Server → source-selection, Client → transfer-mode?mode=client)
- [x] 1.3 Create `ViewModels/ModeSelectionViewModel.cs` with mode selection logic

## 2. App Shell Registration

- [x] 2.1 Replace `HomePage` ShellContent with `ModeSelectionPage` in `AppShell.xaml`
- [x] 2.2 Register `mode-selection` route in `AppShell.xaml.cs`; keep `home` route
- [x] 2.3 Register `ModeSelectionPage` and `ModeSelectionViewModel` in DI via `MauiProgram.cs`
- [x] 2.4 Keep `HomePage` accessible as a navigable route for transfer history

## 3. File View Filter Fix

- [x] 3.1 Add `AllFiles` backing list to `FileSelectionViewModel` that stores the full file set
- [x] 3.2 Update `LoadFromFileSystem` to populate both `AllFiles` and `Files`
- [x] 3.3 Update `LoadFilesAsync` to populate both `AllFiles` and `Files`
- [x] 3.4 Fix `ApplyFilter` to always filter from `AllFiles` instead of the current `Files` collection
- [x] 3.5 Fix `OnFilterAllClicked` to restore from `AllFiles` instead of reloading from disk
- [x] 3.6 Verify filter button visual state highlights active filter correctly

## 4. Build and Verify

- [x] 4.1 Build project with 0 errors
- [x] 4.2 Verify navigation: Server → source selection → file selection with working filters
- [x] 4.3 Verify navigation: Client → transfer mode with client pre-selected (via `?mode=client` query param)
