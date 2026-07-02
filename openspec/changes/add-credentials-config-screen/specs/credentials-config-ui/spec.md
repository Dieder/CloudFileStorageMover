## ADDED Requirements

### Requirement: Credentials config page is accessible from shell toolbar
The system SHALL provide a `CredentialsConfigPage` that users can navigate to via a Shell toolbar item visible from key screens.

#### Scenario: Navigate to credentials config via toolbar
- **WHEN** the user taps the settings gear icon in the shell toolbar
- **THEN** the app navigates to the credentials configuration page

### Requirement: Credentials page lists configurable providers
The system SHALL list all storage providers that support credential configuration (Google Drive and Amazon Prime) on the credentials page.

#### Scenario: View configurable providers
- **WHEN** the user opens the credentials configuration page
- **THEN** the page displays a list of configurable providers showing their name and current configuration status

### Requirement: Credentials page shows per-provider credential fields
The system SHALL display the appropriate credential input fields for the selected provider.

#### Scenario: Enter Google Drive credentials
- **WHEN** the user selects Google Drive from the provider list
- **THEN** the page shows fields for Client ID and Client Secret

#### Scenario: Enter Amazon Prime credentials
- **WHEN** the user selects Amazon Prime from the provider list
- **THEN** the page shows fields for API Key and API Secret

### Requirement: Credentials can be saved and cleared
The system SHALL allow users to save entered credentials or clear existing ones per provider.

#### Scenario: Save credentials
- **WHEN** the user fills in credential fields and taps Save
- **THEN** the credentials are persisted via the credentials service and the provider shows as configured

#### Scenario: Clear credentials
- **WHEN** the user taps Clear on a configured provider
- **THEN** the stored credentials are removed and the provider shows as unconfigured
