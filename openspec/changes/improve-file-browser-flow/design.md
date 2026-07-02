## Context

The current file browser flow for local device source has several gaps:

1. **Mock data is generic** — `MockFileMoverService.GetLocationsAsync` returns the same three locations (Uploads, Documents, Photos) for all providers. For "Local Device", real Windows user folders (Downloads, Pictures, Videos, Music, Desktop) are expected.
2. **No folder drill-down** — `FileSelectionPage` lists files but clicking a folder does nothing. Users expect to navigate into subfolders.
3. **Source selection navigates to destination too early** — After selecting a source folder, the flow goes straight to destination selection. For local browsing, users expect to see folder contents first.
4. **Filter toolbar has no active state** — The filter buttons (All/Images/Documents/Videos) work but don't indicate which filter is currently active.

## Goals / Non-Goals

**Goals:**
- Show standard Windows user folders (Downloads, Documents, Pictures, Videos, Music, Desktop) when "Local Device" is the source
- Allow users to drill into folders on FileSelectionPage (tap a folder → show its contents)
- Change source selection Next flow to navigate to file selection page when source is local
- Show active filter state on the filter toolbar buttons
- Update mock data to support subfolder listing

**Non-Goals:**
- Not implementing real OS file system access (remains mock-driven for now)
- Not changing the cloud-to-cloud transfer flow
- Not adding drag-and-drop or multi-select beyond checkboxes

## Decisions

| Decision | Rationale |
|---|---|
| **Add folder drill-down via existing `ListFilesAsync`** | Reuses the existing `LoadFilesAsync` pattern. When user taps a folder in FileSelectionPage, call `LoadFilesAsync(locationId, folderPath)` to load subfolder contents. |
| **Route source→file selection for local; keep source→destination for cloud** | Minimal change. In `SourceSelectionPage.OnNextClicked`, check if the selected provider is "local" and route to `file-selection` instead of `destination-selection`. The session stores the source path for file loading. |
| **Use `IFileMoverService` to return subfolder contents** | Mock service already has `ListFilesAsync` with a `path` parameter for subfolder navigation. The mock will return different files per path. |
| **Add visual active state to filter buttons** | Modify `FileSelectionPage.xaml` filter button styles to highlight the active filter (e.g., set `BackgroundColor` to the primary color for active, light gray for inactive). |

## Risks / Trade-offs

- **[Mock data limitation]** The folder structure is hardcoded. When real file system access is added later, the folder drill-down UX will remain the same, but the data source changes.
- **[Flow change for local]** Changing the navigation order for local source may confuse cloud-to-cloud users initially. Mitigation: the flow difference is intentional — local needs browsing, cloud doesn't.
