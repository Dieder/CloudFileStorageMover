## 1. Setup

- [x] 1.1 Create a new Maui application or module for `file-mover-ui`
- [x] 1.2 Add required Maui dependencies and configure project scaffolding, including `CommunityToolkit.Maui`, `UraniumUI`, `Camera.MAUI`, and an image editing/cropping library such as `MAUI.ImageCropper`

## 2. Core UI Implementation

- [x] 2.1 Implement source storage selection UI and integration with backend discovery endpoint
- [x] 2.2 Implement destination storage selection UI and mapping to available destination endpoints
- [x] 2.3 Implement client/server mode selection and WiFi transfer settings (server IP/port, listen port)
- [x] 2.4 Implement server QR code display and client QR code scan connection setup
- [x] 2.5 Implement Windows desktop alternate connection method (e.g., USB pairing)
- [x] 2.6 Implement file/folder browsing, item selection, and folder picker support for image or document folders
- [x] 2.7 Implement image editing support for rotate/crop and save edited images as versioned files
- [x] 2.8 Implement document, image, and video selection views with checkbox controls and a toolbar for select none/custom/all
- [x] 2.9 Implement active preview pane for selected files, showing image EXIF metadata and embedded video playback
- [x] 2.10 Implement video playback preview for supported video formats
- [x] 2.11 Implement the review-and-submit move workflow and WiFi file transfer logic

## 3. Transfer Status and History

- [x] 3.1 Implement transfer progress polling or status updates in the UI
- [x] 3.2 Build transfer history and error reporting views

## 4. Integration and Validation

- [x] 4.1 Connect the Maui UI to backend move initiation and status APIs
- [x] 4.2 Validate UI behavior against the file mover UI spec requirements
