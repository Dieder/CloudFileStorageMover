## Context

The existing project is a cloud storage file mover solution. It currently lacks a modern browser-based UI for selecting source/destination storage, choosing files or folders, and monitoring transfer progress.

This change introduces a Maui frontend that connects to the existing file mover backend services and exposes user-friendly move workflows.

## Goals / Non-Goals

**Goals:**
- Provide an Maui-based UI for file mover workflows.
- Enable users to select source and destination storage locations, choose files/folders, and start file moves.
- Support client/server mode for direct WiFi transfers between devices.
- Allow the server to present a QR code for connection setup and the client to scan it.
- Support an alternate Windows desktop connection method such as USB when QR scanning is not available.
- Support a folder picker mode for selecting photos, images, or document directories.
- Display transfer progress, recent transfer history, and errors.
- Integrate with existing backend APIs for cloud storage selection and transfer orchestration.

**Non-Goals:**
- Implement backend transfer orchestration or storage connectors from scratch.
- Build a separate CLI or desktop client.
- Replace existing backend authentication flows.

## Decisions

- **Framework:** Use Maui because the request explicitly specifies a Maui application and the project needs a structured frontend framework for reusable components.
- **Application structure:** Create a dedicated `file-mover-ui` Maui app/module. This will isolate UI concerns from the backend and allow independent deployment.
- **UI tooling:** Use `CommunityToolkit.Maui` for core Maui helpers, `UraniumUI` for presentation components and polished UI elements, and `Camera.MAUI` for camera preview and photo capture.
- **Transfer mode:** Support both client mode and server mode. Client mode sends selected files over WiFi to a remote server address. Server mode listens on a TCP port and receives incoming file streams.
- **Connection setup:** The server should display a QR code encoding connection details (IP, port, session token), and the client should be able to scan that QR code to establish the WiFi transfer.
- **Windows desktop fallback:** Support an alternate connection flow for Windows desktop clients, such as USB pairing or direct cable transfer, when QR scanning is not available.
- **WiFi transfer reference:** Use the `WifiFileTransfer/app` example from `leavesCZY/WifiFileTransfer` as a technical reference for WiFi client/server file transfer.
- **Video transfer & playback:** Support video files like images with thumbnail previews and optional playback. Use `itenfay/QPlayer` as a reference for local video playback and supported media formats.
- **Image editing:** Support image rotate and crop operations. Edited images should be saved as new files in the current folder, using the original name with a version suffix.
- **Folder picker support:** Include a folder picker experience that can target photo/image collections and document folders, while preserving the existing file selection workflow.
- **Active preview panel:** Show metadata and preview content for the currently active file. For images, display EXIF metadata; for videos, display an embedded player.
- **API contract:** The UI will consume backend endpoints for:
  - listing available source storage providers and destinations,
  - resolving available files/folders,
  - initiating a move,
  - polling or subscribing to transfer status,
  - retrieving recent transfer history.
- **State management:** Use Maui services and component state rather than introducing state management libraries unless complexity increases significantly.
- **Routing:** Provide route-based pages for `select-source`, `select-destination`, `mode-selection`, `review-move`, and `transfer-status`.

## Risks / Trade-offs

- [Risk] UI/backend contract mismatch → Mitigation: define a minimal API surface and implement fallback handling for missing fields.
- [Risk] Maui adds frontend dependency complexity → Mitigation: keep the Maui app lean and avoid heavy third-party UI frameworks unless needed.
- [Risk] Deployment complexity if backend and UI are hosted separately → Mitigation: document deployment expectations and support serving the UI from the same host if desired.

## Open Questions

- Which backend endpoints already exist for storage provider discovery and transfer initiation?
- Should the Angular app be served from the same origin as existing services, or deployed as a static SPA separately?
- Does this UI need authentication handling, or can it reuse the backend session mechanism directly?
