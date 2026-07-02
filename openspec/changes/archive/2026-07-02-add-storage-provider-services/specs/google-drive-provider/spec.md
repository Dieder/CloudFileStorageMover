## ADDED Requirements

### Requirement: Google Drive provider authenticates via OAuth2

The system SHALL provide a `GoogleDriveStorageProvider` class that authenticates using OAuth2 with a `client_secrets.json` file and uses the `Google.Apis.Drive.v3` SDK.

#### Scenario: Authenticate with client secrets
- **WHEN** the provider is initialized
- **THEN** it loads `client_secrets.json`, authorizes via `GoogleWebAuthorizationBroker`, and creates a `DriveService` instance

### Requirement: Google Drive provider lists folders

The system SHALL list folders in a Google Drive directory using the Drive v3 API `Files.List` with `mimeType` filter for folders.

#### Scenario: List Google Drive folders
- **WHEN** `ListFoldersAsync(parentId)` is called
- **THEN** the method calls `service.Files.List()` filtered by `mimeType = 'application/vnd.google-apps.folder'` and returns folder names and IDs

### Requirement: Google Drive provider lists files

The system SHALL list files in a Google Drive directory using the Drive v3 API `Files.List`.

#### Scenario: List files in a Google Drive folder
- **WHEN** `ListFilesAsync(folderId)` is called
- **THEN** the method calls `service.Files.List()` with `q = $"'{folderId}' in parents"` and returns file metadata

### Requirement: Google Drive provider uploads files

The system SHALL upload files to Google Drive using `FilesResource.CreateMediaUpload`.

#### Scenario: Upload file to Google Drive
- **WHEN** `UploadFileAsync(localPath, parentFolderId)` is called
- **THEN** the method creates a `FilesResource.CreateMediaUpload` request with the file stream and uploads it

### Requirement: Google Drive provider renames files

The system SHALL rename files or folders in Google Drive by updating the file name via the Drive v3 API.

#### Scenario: Rename a Google Drive item
- **WHEN** `RenameAsync(fileId, newName)` is called
- **THEN** the method creates an update request with the new name and patches the file metadata

### Requirement: Google Drive provider deletes files

The system SHALL delete files or folders in Google Drive using `FilesResource.DeleteRequest`.

#### Scenario: Delete a Google Drive item
- **WHEN** `DeleteAsync(fileId)` is called
- **THEN** the method calls `service.Files.Delete(fileId)` and executes the request
