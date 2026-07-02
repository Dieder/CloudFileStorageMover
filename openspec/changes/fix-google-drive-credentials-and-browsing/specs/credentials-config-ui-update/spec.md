## ADDED Requirements

### Requirement: Credential ID fields are visible
The system SHALL display the Client ID (or API Key) field without password masking in the credentials config UI so users can see their configured value.

#### Scenario: Client ID is visible
- **WHEN** the user opens the credentials config page and selects Google Drive
- **THEN** the Client ID field displays the saved value in plain text

#### Scenario: Client Secret remains masked
- **WHEN** the user opens the credentials config page and selects Google Drive
- **THEN** the Client Secret field displays dots instead of the actual value
