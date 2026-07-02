## Context

The app currently opens on a `HomePage` showing a "Start New Transfer" button and recent transfer history. Users must click through to source selection before choosing a transfer mode. The `TransferModePage` already has Server/Client radio buttons but appears too late in the flow (after source selection and file selection). 

The `FileSelectionPage` has filter buttons (All, Images, Documents, Videos) but `ApplyFilter()` filters from the current in-memory `Files` collection rather than the full file list. This means clicking "Images" then "Documents" produces an empty result (documents filtered from the image-only set). The "All" button works because it reloads from the file system first.

Google Drive and Amazon Prime source providers don't show any content because `GoogleDriveStorageProvider` requires OAuth2 credentials that aren't wired from `ICredentialsService` (being addressed in `fix-google-drive-credentials-and-browsing`).

## Goals / Non-Goals

**Goals:**
- Server/Client mode selection as the app's initial screen
- Server mode routes to source selection → file browsing with working filters
- Client mode routes to WiFi connection flow (QR scan, USB pairing)
- Fix file view filter buttons to correctly filter photos, videos, and documents from the full file set
- Fix filter button visual state to highlight the active filter

**Non-Goals:**
- Not implementing actual Google Drive/Prime OAuth2 authentication (separate change)
- Not modifying the transfer execution flow
- Not removing HomePage entirely (keep as transfer history view)

## Decisions

| Decision | Rationale |
|---|---|
| **New `ModeSelectionPage` as ShellContent** | Simpler than modifying app startup logic. Replace HomePage as the default TabBar content. |
| **Server mode navigates to `source-selection`** | Reuses the existing `SourceSelectionPage` which already supports provider selection and location browsing. |
| **Client mode navigates to `transfer-mode` with client preset** | The `TransferModePage` already has the Client radio button and QR scan UI. Pre-select the Client option. |
| **Store full file list in `FileSelectionViewModel.AllFiles`** | Keeps the full set separate from the displayed `Files` collection. Filters always operate on the full list. |
| **Remove `OnFilterAllClicked` file system reload** | With a proper `AllFiles` backing list, "All" simply restores the unfiltered view without a disk reload. |
| **Keep HomePage as a route** | Avoids breaking existing navigation. Users can still reach transfer history. |

## Risks / Trade-offs

- **[Navigation depth change]** Replacing the entry page changes the navigation stack root. Mitigation: test all back-navigation paths from source selection and file selection.
- **[Client mode flow]** Client mode currently needs a server address and port. Mitigation: navigate to `transfer-mode` with client pre-selected and QR scan available.
