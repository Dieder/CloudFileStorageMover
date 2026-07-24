## ADDED Requirements

### Requirement: Google Photos Picker button in shell menu

The system SHALL display a Google Photos Picker icon button in the left menu bar, positioned between the Source and Transfer buttons.

#### Scenario: Picker button visible in menu
- **WHEN** the app shell is rendered
- **THEN** the left menu bar displays a Google Photos icon button (📷 or similar) that is clickable

### Requirement: Google Photos Picker creates session and opens browser

The system SHALL create a Google Photos Picker session via the `photospicker.googleapis.com/v1/sessions` API and open the picker URL in the user's default browser.

#### Scenario: Picker session created and browser opened
- **WHEN** the user clicks the Google Photos Picker button in the left menu
- **THEN** the system creates a picker session using the user's Google access token, opens `https://photos.google.com/create-session?sessionId={sessionId}` in the default browser, and displays a loading state

#### Scenario: OAuth scope includes photoslibrary.readonly
- **WHEN** the picker flow initiates OAuth
- **THEN** the system requests scope `https://www.googleapis.com/auth/photoslibrary.readonly`

### Requirement: Google Photos Picker polls for selected media

The system SHALL poll the `photospicker.googleapis.com/v1/mediaItems` endpoint to retrieve photos the user has selected in the browser picker.

#### Scenario: Media items retrieved after selection
- **WHEN** the user completes selection in the browser and the system polls the media items endpoint
- **THEN** the system retrieves the list of selected media items with their `baseUrl`, filename, and metadata

#### Scenario: Polling timeout
- **WHEN** the user does not complete selection within 120 seconds
- **THEN** the system stops polling and displays a timeout message, returning to the previous state

### Requirement: Google Photos Picker downloads selected photos to local folder

The system SHALL download all selected photos from the picker to the user's configured images folder (from `DestinationSettingsService` or `Environment.SpecialFolder.MyPictures`).

#### Scenario: Photos downloaded to images folder
- **WHEN** media items are retrieved from the picker session
- **THEN** the system downloads each photo using the `baseUrl` with `=d` suffix (for download) and saves it to the images destination folder with its original filename

#### Scenario: Download with progress feedback
- **WHEN** photos are being downloaded
- **THEN** the system displays a progress indicator showing the number of files downloaded out of total

#### Scenario: Destination folder does not exist
- **WHEN** the target images folder does not exist on disk
- **THEN** the system creates the folder before downloading

### Requirement: Google Photos Picker handles errors gracefully

The system SHALL handle network errors, auth failures, and empty selections with appropriate user feedback.

#### Scenario: Auth token expired during picker flow
- **WHEN** the access token expires during session creation or polling
- **THEN** the system attempts to refresh the token using the refresh token, and if that fails, prompts the user to sign in again

#### Scenario: User closes browser without selecting
- **WHEN** the picker session returns zero media items after polling
- **THEN** the system displays "No photos selected" and returns to the previous state
