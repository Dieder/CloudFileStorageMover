## Why

After using the image editor from the file selection screen, pressing back/cancel returns to the wrong page. The user expects to return to the source selection screen, but the current navigation stack leads back to file selection after editing, breaking the expected app flow.

## What Changes

- Fix the ImageEditorPage Cancel button to navigate back to the source selection screen (2 levels up) instead of returning to FileSelectionPage
- Fix the FileSelectionPage "Back" button to reliably return to SourceSelectionPage regardless of prior navigation history (image editor, folder navigation, etc.)
- Ensure the ImageEditorPage preserves proper Shell navigation state so hardware/software back buttons also behave correctly

## Capabilities

### New Capabilities
- `edit-image-navigation`: Correct back-navigation behavior for the image editor flow, ensuring users return to source selection after editing an image

### Modified Capabilities

- (none)

## Impact

- `Pages/ImageEditorPage.xaml.cs` — Update Cancel navigation target
- `Pages/FileSelectionPage.xaml.cs` — Verify/adjust Back button navigation for robustness
- Navigation flow between SourceSelection, FileSelection, and ImageEditor pages
