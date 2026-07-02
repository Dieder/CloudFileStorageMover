## ADDED Requirements

### Requirement: Credentials service provides secure storage
The system SHALL provide an `ICredentialsService` interface with methods for saving, loading, and clearing credentials per provider.

#### Scenario: Save provider credentials
- **WHEN** `SaveCredentialsAsync(providerId, credentials)` is called
- **THEN** the credentials dictionary is stored securely for the given provider ID

#### Scenario: Load provider credentials
- **WHEN** `GetCredentialsAsync(providerId)` is called
- **THEN** the stored credentials dictionary is returned, or null if none exist

#### Scenario: Clear provider credentials
- **WHEN** `ClearCredentialsAsync(providerId)` is called
- **THEN** all stored credentials for the given provider ID are removed

### Requirement: Credentials service checks configuration status
The system SHALL provide a method to check whether a provider has stored credentials.

#### Scenario: Check if provider is configured
- **WHEN** `IsConfiguredAsync(providerId)` is called
- **THEN** true is returned if credentials exist for that provider, false otherwise

### Requirement: Credentials are stored using SecureStorage
The system SHALL use `SecureStorage` as the primary storage mechanism with an encrypted file fallback for platforms where SecureStorage is unavailable.

#### Scenario: Store via SecureStorage
- **WHEN** credentials are saved
- **THEN** they are stored using `SecureStorage.SetAsync` with a provider-prefixed key

#### Scenario: Fallback to encrypted file
- **WHEN** `SecureStorage` is unavailable on the current platform
- **THEN** credentials are stored in a local encrypted JSON file in the app data directory
