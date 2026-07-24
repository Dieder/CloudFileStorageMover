## MODIFIED Requirements

### Requirement: Google Drive provider lists folders

The system SHALL list folders in a Google Drive directory using the Drive v3 API `Files.List` with `q = '{folderId} in parents and mimeType = 'application/vnd.google-apps.folder''`.

#### Scenario: List Google Drive folders
- **WHEN** `ListFoldersAsync(folderId)` is called with a valid Drive folder ID (or `"root"` for the root folder)
- **THEN** the method queries `https://www.googleapis.com/drive/v3/files?q='{folderId}' in parents and mimeType='application/vnd.google-apps.folder'` with the Bearer access token and returns folder names and IDs as `FileItem` objects

#### Scenario: List root folder contents
- **WHEN** `ListFoldersAsync("root")` is called
- **THEN** the method resolves `"root"` as the Drive root folder and returns all top-level folders

### Requirement: Google Drive provider lists files

The system SHALL list files in a Google Drive directory using the Drive v3 API `Files.List` with `q = '{folderId} in parents'` excluding folders.

#### Scenario: List files in a Google Drive folder
- **WHEN** `ListFilesAsync(folderId)` is called with a valid Drive folder ID
- **THEN** the method queries `https://www.googleapis.com/drive/v3/files?q='{folderId}' in parents and mimeType != 'application/vnd.google-apps.folder'` and returns file metadata as `FileItem` objects

### Requirement: Google Drive provider returns common directories

The system SHALL return a single common directory entry representing the Drive root folder.

#### Scenario: Get common directories
- **WHEN** `GetCommonDirectoriesAsync()` is called
- **THEN** the method returns a list containing one entry: `"root|Google Drive"` where `"root"` is the Drive root folder ID and `"Google Drive"` is the display name
