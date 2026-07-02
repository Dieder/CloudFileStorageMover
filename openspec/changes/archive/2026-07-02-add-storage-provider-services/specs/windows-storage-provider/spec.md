## ADDED Requirements

### Requirement: Windows storage provider uses System.IO

The system SHALL provide a `WindowsStorageProvider` class that implements `IStorageProviderService` using `System.IO` for local file system access.

#### Scenario: List local folders
- **WHEN** `ListFoldersAsync(path)` is called
- **THEN** the method uses `Directory.EnumerateDirectories` to return subfolder names

#### Scenario: List local files
- **WHEN** `ListFilesAsync(path)` is called
- **THEN** the method uses `Directory.EnumerateFiles` to return file metadata (name, size, modified date)

#### Scenario: Create local file
- **WHEN** `CreateFileAsync(path, contentStream)` is called
- **THEN** the method writes the stream to the specified local path using `FileStream`

#### Scenario: Upload local file
- **WHEN** `UploadFileAsync(localPath, remotePath)` is called
- **THEN** the method copies the file from `localPath` to `remotePath` using `File.Copy`

#### Scenario: Rename local file or folder
- **WHEN** `RenameAsync(oldPath, newName)` is called
- **THEN** the method uses `File.Move` or `Directory.Move` to rename the item

#### Scenario: Delete local file or folder
- **WHEN** `DeleteAsync(path)` is called
- **THEN** the method uses `File.Delete` or `Directory.Delete` to remove the item
