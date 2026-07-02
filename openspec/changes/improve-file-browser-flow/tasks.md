## 1. Mock Data Updates

- [ ] 1.1 Add standard Windows user folders (Downloads, Documents, Pictures, Videos, Music, Desktop) to `MockFileMoverService.GetLocationsAsync` when provider is "local"
- [ ] 1.2 Update `MockFileMoverService.ListFilesAsync` to return different file sets based on the path parameter (simulate subfolder contents)

## 2. Source Selection Routing

- [ ] 2.1 In `SourceSelectionPage.xaml.cs`, change `OnNextClicked` to navigate to `file-selection` when the selected provider is "Local Device", passing the selected location as context
- [ ] 2.2 Store the selected location path in `TransferSessionService` for file selection page to use

## 3. Folder Drill-Down

- [ ] 3.1 Add a tap handler in `FileSelectionPage.xaml.cs` that detects when a folder is tapped and calls `LoadFilesAsync` with the folder's path
- [ ] 3.2 Add a back navigation method in `FileSelectionViewModel` to return to the parent folder (track current path stack)
- [ ] 3.3 Add a "Back to parent folder" button or breadcrumb in `FileSelectionPage.xaml`

## 4. Filter Toolbar Active State

- [ ] 4.1 Update filter button styles in `FileSelectionPage.xaml` to support active/inactive visual states
- [ ] 4.2 Add active filter tracking in `FileSelectionPage.xaml.cs` and update button backgrounds on filter change

## 5. Verification

- [ ] 5.1 Build the project for Windows target and confirm no compilation errors
- [ ] 5.2 Run the app and verify: local source shows common folders, folder drill-down works, filter toolbar shows active state
