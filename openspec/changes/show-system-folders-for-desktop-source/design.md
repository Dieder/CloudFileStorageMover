## Context

The FileSelectionViewModel has a `LoadFromFileSystem` method that enumerates directories and files at a given path. Its `AllFiles` list stores the full file set. When the user selects Desktop as the source location, only the raw desktop contents are shown. The user expects to see well-known system folders (Downloads, Documents, Pictures) as root-level navigable entries — similar to Windows File Explorer's quick access.

The existing `NavigateToFolder` method pushes paths onto a stack and calls `LoadFromFileSystem`, so clicking a system folder entry will simply load that folder's real path.

## Goals / Non-Goals

**Goals:**
- When the Desktop location (`local-desktop`) is loaded, inject system folder entries at the top of the file list
- System folders: Downloads, Documents, Pictures, Videos, Music — mapped to their real `Environment.SpecialFolder` paths
- Clicking a system folder navigates into it via `NavigateToFolder`
- Actual Desktop contents appear below the system folder entries

**Non-Goals:**
- Changing the source selection page or location list
- Modifying non-Desktop locations (Downloads, Documents, Pictures are already direct paths)
- Adding a separate "system folders" section for non-Desktop locations
- Persisting or caching system folder entries across page navigations

## Decisions

- **Decision**: Inject system folder entries directly into `AllFiles` at load time when the location is `local-desktop`
  - **Rationale**: Simplest approach. `AllFiles` is already the full file set used for filtering. System folders are just `FileItem` entries with `IsFolder = true`. No new data structures or UI changes needed.
  - **Alternative considered**: A separate `SystemFolders` collection rendered via a different section in the XAML — adds UI complexity and template duplication.

- **Decision**: Detect desktop location by comparing the `CurrentPath` to the known `SpecialFolder.Desktop` path
  - **Rationale**: The location ID (`local-desktop`) isn't available inside the ViewModel after the initial load. Comparing the current directory path against `Environment.GetFolderPath(SpecialFolder.Desktop)` is reliable and avoids passing extra context.
  - **Alternative considered**: Passing a flag via a property set from the page code-behind — adds coupling.

- **Decision**: System folder `FileItem` entries use the real filesystem path as `Path` and `Id` so `NavigateToFolder` works unchanged
  - **Rationale**: `NavigateToFolder` pushes the current path and calls `LoadFromFileSystem(folder.Path)`. System folder entries with real paths integrate seamlessly.

## Risks / Trade-offs

- [Desktop path comparison may fail on non-English Windows] → Use `Path.GetFullPath()` normalization and OrdinalIgnoreCase comparison
- [System folders might not exist (e.g., Music folder removed)] → Guard with `Directory.Exists()` before adding; skip missing folders silently
- [User sees duplicate entries if they also have actual Downloads/Pictures folders on their Desktop] → Acceptable — the virtual entries appear first, and actual Desktop subfolders follow below; naming is identical so behavior is clear
