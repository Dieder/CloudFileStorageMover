## ADDED Requirements

### Requirement: File mover UI provides source storage selection
The system SHALL allow users to choose a source cloud storage provider or location from a list of available sources.

#### Scenario: Display available source locations
- **WHEN** the user opens the file mover UI
- **THEN** the UI displays the configured source storage providers and available connection endpoints

### Requirement: File mover UI provides destination storage selection
The system SHALL allow users to choose a destination cloud storage provider or location for the selected files.

#### Scenario: Display available destination locations
- **WHEN** the user reaches the destination selection step
- **THEN** the UI displays destination storage providers and destination locations supported by the backend

### Requirement: File mover UI allows selecting files or folders
The system SHALL allow users to browse and select files or folders from the chosen source location, including a folder picker experience for photo and document directories.

#### Scenario: Select files to move
- **WHEN** the user selects a source storage location
- **THEN** the UI shows the file and folder tree for that source and allows selection of one or more items

#### Scenario: Use folder picker for photos or documents
- **WHEN** the user opens the folder picker and selects an image or document folder
- **THEN** the UI includes that folder and its contents in the move request

### Requirement: File mover UI shows document selection view
The system SHALL show a document file view with document-type icon, filename, date, and a checkbox for each file.

#### Scenario: Display document list with dates
- **WHEN** the user opens the document selection view
- **THEN** the UI lists document files with a document icon, filename, and date
- **AND** each document has a checkbox for multi-selection

#### Scenario: Document selection toolbar
- **WHEN** the user uses the toolbar controls
- **THEN** the UI can select none, select all, or allow custom checkbox selection of the documents in the view

### Requirement: File mover UI supports camera photo capture
The system SHALL allow the user to take a photo from within the app and save the captured image into the currently selected folder.

#### Scenario: Capture photo from toolbar
- **WHEN** the user taps the camera action in the toolbar above the file list or menu
- **THEN** the app opens the camera preview, captures a photo, and saves it to the current folder

#### Scenario: Photo appears in current folder
- **WHEN** the user saves a captured photo
- **THEN** the photo appears as a file in the current folder and is available for selection and transfer

### Requirement: File mover UI shows image thumbnail selection view
The system SHALL show supported image files (`gif`, `jpg`, `jpeg`, `png`) as thumbnails with a checkbox for each item.

#### Scenario: Display image thumbnails
- **WHEN** the user opens the image selection view
- **THEN** the UI displays image thumbnails for supported files and includes a checkbox for each item

#### Scenario: Image selection toolbar
- **WHEN** the user uses the toolbar controls
- **THEN** the UI can select none, select all, or allow custom checkbox selection of the images in the view

### Requirement: File mover UI supports image rotate and crop
The system SHALL allow users to rotate and crop selected image files before transfer.

#### Scenario: Rotate and crop an image
- **WHEN** the user selects an image and opens the image editor
- **THEN** the UI allows rotating and cropping the image and saving the edited result

#### Scenario: Save edited image with version suffix
- **WHEN** the user saves a rotated or cropped image
- **THEN** the app stores a new file in the current folder with the original filename plus a version suffix

### Requirement: File mover UI shows video thumbnail selection view
The system SHALL show supported video files as thumbnails with a checkbox for each item, and allow playing the selected video in a preview pane.

#### Scenario: Display video thumbnails
- **WHEN** the user opens the video selection view
- **THEN** the UI displays thumbnails for supported video files and includes a checkbox for each item

#### Scenario: Video playback preview
- **WHEN** the user selects a video thumbnail and opens the preview
- **THEN** the UI plays the video using an embedded player for supported formats such as `m4v`, `wmv`, `mp4`, `mov`, `avi`, `mkv`, `flv`, `rm`, and `rmvb`

### Requirement: File mover UI shows active file preview
The system SHALL display an active file preview pane when a file is selected.

#### Scenario: Show image EXIF metadata
- **WHEN** the user selects an image file
- **THEN** the UI shows an EXIF metadata view alongside the image preview

#### Scenario: Show video player for selected videos
- **WHEN** the user selects a video file
- **THEN** the UI shows the embedded video player in the active preview pane

#### Scenario: Active preview updates with selection
- **WHEN** the user changes the active file selection
- **THEN** the preview pane updates to show the appropriate image EXIF or video player

#### Scenario: Selection toolbar still applies
- **WHEN** the user uses the selection toolbar while a file is active
- **THEN** the preview pane remains visible and updates if the active file changes

#### Scenario: Active media is transferable
- **WHEN** the user selects a video or image and confirms transfer
- **THEN** the selected active file is included in the transfer request

#### Scenario: Support active previews for all supported media
- **WHEN** the user selects supported image or video formats
- **THEN** the UI displays the correct preview type for that media

#### Scenario: Preserve preview for non-media files
- **WHEN** the user selects a document file
- **THEN** the preview pane can show file metadata or placeholder details as needed

#### Scenario: Active preview uses camera-captured files
- **WHEN** the user captures a photo and it appears in the current folder
- **THEN** the photo can be selected and its EXIF preview shown immediately

#### Scenario: Active preview supports edited image versions
- **WHEN** the user saves a rotated or cropped image version
- **THEN** the new version can be selected and its EXIF preview shown in the pane

#### Scenario: Active preview for videos persists through transfer
- **WHEN** the user transfers a selected video file
- **THEN** the active video preview remains available until the transfer completes or the selection changes

#### Scenario: Active previews do not block toolbar actions
- **WHEN** the user selects none/all using the toolbar
- **THEN** the preview pane updates accordingly and remains functional

### Requirement: File mover UI initiates transfer operations

### Requirement: File mover UI initiates transfer operations
The system SHALL allow users to review selected items and start the move operation to the chosen destination.

#### Scenario: Start file move
- **WHEN** the user submits the move request from the review screen
- **THEN** the backend receives the move request and the UI transitions to a transfer status view

### Requirement: File mover UI supports client/server WiFi transfer mode
The system SHALL allow the user to choose between client mode (send files) and server mode (receive files) over WiFi.

#### Scenario: Client mode send over WiFi
- **WHEN** the user selects client mode, scans a server QR code, and submits the transfer
- **THEN** the app sends the selected files using a TCP connection over WiFi and reports progress

#### Scenario: Server mode receive over WiFi
- **WHEN** the user selects server mode and starts listening on the configured port
- **THEN** the app accepts incoming TCP connections over WiFi, receives files, and stores them locally

### Requirement: File mover UI provides QR code connection setup
The system SHALL allow the server to display a QR code with connection metadata and the client to scan it for automatic setup.

#### Scenario: Server displays QR code
- **WHEN** the user starts the app in server mode
- **THEN** the server view displays a QR code containing the connection details required by the client

#### Scenario: Client scans QR code
- **WHEN** the user opens the client connection view and scans the server QR code
- **THEN** the client automatically populates the server connection details and can proceed with the transfer

### Requirement: File mover UI supports alternate Windows desktop connection
The system SHALL allow Windows desktop clients to use an alternate connection method such as USB pairing when QR scanning is unavailable.

#### Scenario: Windows desktop USB fallback
- **WHEN** the user runs the Windows desktop client and selects alternate connection method
- **THEN** the app guides the user through using USB as the transport channel instead of WiFi QR pairing

### Requirement: File mover UI reports transfer status
The system SHALL show transfer progress, success, and failure states for active moves.

#### Scenario: Display transfer progress
- **WHEN** a transfer is in progress
- **THEN** the UI displays progress updates, including completed items and any errors encountered

### Requirement: File mover UI surfaces transfer history and errors
The system SHALL allow users to view recent transfer attempts and any reported errors.

#### Scenario: View past transfers
- **WHEN** the user opens the transfer history view
- **THEN** the UI lists recent move attempts and their final status
