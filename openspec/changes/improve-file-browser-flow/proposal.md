## Why

When selecting "Local Device" as source, common folders like Downloads are missing from the locations list, and clicking Next on a folder takes you to destination selection instead of showing what's inside it. Users expect a file-browser experience where they can drill into folders, see their contents, and filter by file type.

## What Changes

- Add standard Windows user folders (Downloads, Documents, Pictures, Videos, Music, Desktop) to the local device locations in MockFileMoverService
- Change source selection flow for local folders: selecting a folder and clicking Next navigates to the file selection page to browse its files, not directly to destination
- Add folder drill-down navigation in FileSelectionPage (click a folder → load its contents)
- Expand the file type filter toolbar with active state indication so users can see which filter is applied

## Capabilities

### New Capabilities
- `local-folder-browser`: Browse local device folders with drill-down navigation and common folder shortcuts

### Modified Capabilities
- `file-mover-ui`: Update file selection spec to require folder drill-down and enhanced filter toolbar behavior

## Impact

- `Services/MockFileMoverService.cs` — add common local folders to locations, add file/folder listing for subfolder navigation
- `Pages/SourceSelectionPage.xaml.cs` — change Next navigation to go to file selection when source is local
- `Pages/FileSelectionPage.xaml` — enhance filter toolbar with active state styling
- `Pages/FileSelectionPage.xaml.cs` — add folder drill-down (navigate into subfolders), track current filter state
- `ViewModels/FileSelectionViewModel.cs` — add subfolder navigation method, filter tracking
