## Why

Teams need an easy way to move files between cloud storage locations using a modern web UI. An Maui application will simplify file selection, transfer orchestration, and status visibility for end users.

## What Changes

- Introduce a new Maui web application for file mover operations.
- Add user workflows for selecting source and destination storage, choosing files/folders, and starting a transfer.
- Add a folder picker option for selecting photo and document folders.
- Allow users to rotate and crop images and save the edited image as a new versioned file in the current folder.
- Provide camera photo capture from the app and save the photo to the current folder.
- Provide transfer status reporting and error handling within the UI.
- Use `CommunityToolkit.Maui`, `UraniumUI`, and `Camera.MAUI` for UI components and camera integration.
- Support integration points for existing cloud storage mover backend services.

## Capabilities

### New Capabilities
- `file-mover-ui`: UI-driven file transfer and management workflows for selecting source/destination storage, initiating moves, and viewing transfer progress.

### Modified Capabilities
- None

## Impact

- New Maui frontend code and UI components using `CommunityToolkit.Maui` and `UraniumUI`.
- Integration with existing file mover backend APIs and authentication flows.
- Potential updates to deployment/configuration for hosting the Maui app.
- May require coordination with existing storage connector and transfer orchestration services.


[Example sources]
https://github.com/itenfay/QPlayer
https://github.com/leavesCZY/WifiFileTransfer
https://github.com/shogo-tsunoda/file-bridge
https://github.com/grandiloquent/Rust-Android-WIFI-File-Manager
https://github.com/dimonovdd/Xamarin.MediaGallery
https://github.com/lockinfinity123/MAUI.ImageCropper
https://github.com/jfversluis/Plugin.Maui.Exif
https://github.com/nicolasaramayo/FileBridge