## 1. Image Editor Navigation Fix

- [x] 1.1 Update `ImageEditorPage.xaml.cs` Cancel handler: change `GoToAsync("..")` to `GoToAsync("../..")` for 2-level pop to SourceSelectionPage
- [x] 1.2 Add `OnBackButtonPressed` override in `ImageEditorPage.xaml.cs` to navigate to SourceSelectionPage via `GoToAsync("../..")`
- [x] 1.3 Add navigation stack guard: only pop 2 levels if there are enough pages on the stack; otherwise fall back to `GoToAsync("..")`
- [x] 1.4 Update `OnSaveClicked` in `ImageEditorPage.xaml.cs` to navigate to SourceSelectionPage after saving

## 2. File Selection Back Button

- [x] 2.1 Update `FileSelectionPage.xaml.cs` `OnBackClicked` to use `GoToAsync("//mode-selection/source-selection")` for absolute navigation instead of relative `..`

## 3. Build & Verify

- [x] 3.1 Build project with 0 errors
- [x] 3.2 Verify navigation: ImageEditorPage Cancel → SourceSelectionPage (code: `../..` with stack guard)
- [x] 3.3 Verify navigation: ImageEditorPage Save → SourceSelectionPage (code: `GoBackToSourceSelectionAsync()` after save)
- [x] 3.4 Verify navigation: FileSelectionPage Back → SourceSelectionPage (code: absolute route in `OnBackClicked`)
- [x] 3.5 Verify hardware back button on ImageEditorPage also returns to SourceSelectionPage (code: `OnBackButtonPressed` override)
