## ADDED Requirements

### Requirement: Prime storage provider implements IStorageProviderService

The system SHALL provide a `PrimeStorageProvider` class that implements `IStorageProviderService` with the same async contract as other providers.

#### Scenario: List Prime folders
- **WHEN** `ListFoldersAsync(path)` is called
- **THEN** the method returns folder names from the Prime storage service

#### Scenario: List Prime files
- **WHEN** `ListFilesAsync(path)` is called
- **THEN** the method returns file metadata from the Prime storage service

#### Scenario: Create file on Prime
- **WHEN** `CreateFileAsync(path, contentStream)` is called
- **THEN** the file is created on the Prime storage service

#### Scenario: Upload file to Prime
- **WHEN** `UploadFileAsync(localPath, remotePath)` is called
- **THEN** the file is uploaded to the Prime storage service

#### Scenario: Rename Prime item
- **WHEN** `RenameAsync(oldPath, newName)` is called
- **THEN** the file or folder is renamed on the Prime storage service

#### Scenario: Delete Prime item
- **WHEN** `DeleteAsync(path)` is called
- **THEN** the file or folder is deleted from the Prime storage service
