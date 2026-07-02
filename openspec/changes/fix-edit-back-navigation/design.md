## Context

The app uses Shell navigation with registered routes. The current flow is:
1. ModeSelectionPage → "source-selection" → SourceSelectionPage
2. SourceSelectionPage → "file-selection" → FileSelectionPage
3. FileSelectionPage → "image-editor" → ImageEditorPage (via push navigation)

ImageEditorPage.Cancel does `Shell.Current.GoToAsync("..")` which pops one level back to FileSelectionPage. FileSelectionPage's back button also does `GoToAsync("..")` which pops to SourceSelectionPage. This works in the simple case but breaks after the image editor flow because Shell's navigation stack treats the editor as a separate push, and the user must press Back twice to reach source selection. Additionally, hardware/software back buttons may not consistently pop to the correct level.

## Goals / Non-Goals

**Goals:**
- After canceling/saving in the ImageEditorPage, navigate directly back to SourceSelectionPage (skipping FileSelectionPage)
- FileSelectionPage back button reliably returns to SourceSelectionPage regardless of prior navigation
- Hardware/software back buttons on ImageEditorPage also navigate to SourceSelectionPage

**Non-Goals:**
- Changing the file selection or source selection behavior
- Adding new pages or routes
- Preserving image editor state across navigation

## Decisions

- **Decision**: Use `GoToAsync("../..")` from ImageEditorPage to pop 2 levels back to SourceSelectionPage
  - **Rationale**: The image editor is a leaf action (edit and done). Returning to FileSelectionPage after editing forces two back presses. Popping 2 levels in one step is the simplest Shell-native approach without introducing state flags or custom navigation services.
  - **Alternative considered**: Passing a query parameter to FileSelectionPage to auto-navigate back to source selection — adds complexity and intermediate flicker.

- **Decision**: Keep FileSelectionPage.OnBackClicked as `GoToAsync("..")` — no change needed
  - **Rationale**: This already correctly pops to SourceSelectionPage in the standard flow. The bug is only in the ImageEditorPage's navigation target, not in FileSelectionPage itself.

- **Decision**: Override `OnBackButtonPressed` on ImageEditorPage to also navigate via `GoToAsync("../..")`
  - **Rationale**: Android hardware back and Shell's built-in back navigation may not respect the 2-level pop. Explicitly handling the back button ensures consistency across platforms.

## Risks / Trade-offs

- [Hardware back inconsistency] → Override `OnBackButtonPressed` to match Cancel behavior
- [Two-level pop fails if stack is shallow] → Use `Shell.Current.Navigation.NavigationStack` count guard before navigating
- [User may expect to return to file selection after saving an image] → Add a confirmation dialog: "Return to source selection?" with Yes/Cancel options to make intent clear
