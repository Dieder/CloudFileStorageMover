## Why

When users browse files in the source selection screen, clicking on a folder should automatically open it and display its contents, allowing immediate file selection. Currently, users need multiple navigation steps, which disrupts their workflow and makes the app feel less responsive.

## What Changes

- Enable automatic folder navigation: clicking a folder immediately displays its contents
- Implement smooth navigation flow: folder click → instant contents display → immediate file selection
- Reduce manual navigation steps by preloading subfolder contents
- Improve user experience with smoother, more intuitive browsing

## Capabilities

### New Capabilities
- `automatic-folder-navigation`: Click a folder to automatically open it and display contents without additional navigation steps

### Modified Capabilities
- (none)

## Impact

- `ViewModels/FileSelectionViewModel.cs` — Enhance NavigateToFolder to enable automatic navigation
- `Pages/FileSelectionPage.xaml.cs` — Update navigation logic for smoother folder browsing
- User-facing: Navigating into folders should be instant and seamless