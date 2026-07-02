## ADDED Requirements

### Requirement: Source selection shows provider configuration status
The system SHALL display whether each storage provider has credentials configured, and allow navigating to the credentials config page for unconfigured providers.

#### Scenario: Show configured status
- **WHEN** the source selection page loads providers
- **THEN** each provider displays a configured or unconfigured badge

#### Scenario: Configure unconfigured provider
- **WHEN** the user taps an unconfigured provider
- **THEN** the app navigates to the credentials configuration page for that provider
