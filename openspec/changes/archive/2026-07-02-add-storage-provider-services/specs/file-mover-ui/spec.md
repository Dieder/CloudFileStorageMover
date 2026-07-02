## ADDED Requirements

### Requirement: IFileMoverService delegates to provider implementations

The system SHALL update the existing `FileMoverService` to use registered `IStorageProviderService` implementations based on the selected provider type.

#### Scenario: Select provider by type
- **WHEN** the user selects a storage provider (e.g., Google Drive)
- **THEN** `FileMoverService` delegates file operations to the corresponding `IStorageProviderService` implementation

#### Scenario: Registered providers in DI
- **WHEN** the app starts
- **THEN** all `IStorageProviderService` implementations are registered in the dependency injection container via `MauiProgram.cs`
