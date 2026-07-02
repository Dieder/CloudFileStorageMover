## MODIFIED Requirements

### Requirement: Google Drive provider authenticates via OAuth2

The system SHALL provide a `GoogleDriveStorageProvider` class that authenticates using OAuth2 with credentials obtained from `ICredentialsService` and uses the `Google.Apis.Drive.v3` SDK.

#### Scenario: Authenticate with saved credentials
- **WHEN** the provider is initialized
- **THEN** it loads ClientId and ClientSecret from `ICredentialsService`, constructs a `ClientSecrets` object, authorizes via `GoogleWebAuthorizationBroker`, and creates a `DriveService` instance

#### Scenario: Fallback to config file on missing credentials
- **WHEN** `ICredentialsService` has no saved credentials for Google Drive
- **THEN** the provider falls back to the `client_secrets.json` file specified in `config/providers/google-drive.json`
