## ADDED Requirements

### Requirement: Provider configuration is stored in JSON files

The system SHALL load per-provider configuration from JSON files in a `config/providers/` directory.

#### Scenario: Load Google Drive config
- **WHEN** the Google Drive provider is initialized
- **THEN** the system reads `config/providers/google-drive.json` for API and auth settings

#### Scenario: Load Windows provider config
- **WHEN** the Windows provider is initialized
- **THEN** the system reads `config/providers/windows.json` for base directory and settings

#### Scenario: Load Prime provider config
- **WHEN** the Prime provider is initialized
- **THEN** the system reads `config/providers/prime.json` for connection and auth settings

### Requirement: Config model is typed

The system SHALL use a typed C# model class to deserialize provider configuration.

#### Scenario: Config deserialization
- **WHEN** a config file is loaded
- **THEN** its JSON content is deserialized into a strongly-typed `ProviderConfig` object
