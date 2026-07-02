## ADDED Requirements

### Requirement: Storage provider interface defines async file operations

The system SHALL provide an `IStorageProviderService` interface with async methods for listing, creating, uploading, renaming, and deleting files and folders.

#### Scenario: List folders in a directory
- **WHEN** the caller invokes `ListFoldersAsync(path)` on a provider
- **THEN** the method returns a collection of folder names at the given path

#### Scenario: List files in a directory
- **WHEN** the caller invokes `ListFilesAsync(path)` on a provider
- **THEN** the method returns a collection of file metadata at the given path

#### Scenario: Create a file
- **WHEN** the caller invokes `CreateFileAsync(path, contentStream)` on a provider
- **THEN** the file is created at the specified path with the provided content

#### Scenario: Upload a file
- **WHEN** the caller invokes `UploadFileAsync(localPath, remotePath)` on a provider
- **THEN** the file from `localPath` is uploaded to `remotePath` on the provider

#### Scenario: Rename a file or folder
- **WHEN** the caller invokes `RenameAsync(oldPath, newName)` on a provider
- **THEN** the file or folder at `oldPath` is renamed to `newName`

#### Scenario: Delete a file or folder
- **WHEN** the caller invokes `DeleteAsync(path)` on a provider
- **THEN** the file or folder at the given path is deleted
