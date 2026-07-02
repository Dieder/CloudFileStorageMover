## ADDED Requirements

### Requirement: App starts with Server/Client mode selection
The system SHALL present a mode selection screen as the app's initial page, allowing the user to choose between Server mode and Client mode before any other interaction.

#### Scenario: App opens to mode selection
- **WHEN** the application starts
- **THEN** the mode selection screen is displayed with "Server" and "Client" options

#### Scenario: Server mode navigates to source selection
- **WHEN** the user selects "Server" mode
- **THEN** the app navigates to the source storage selection page

#### Scenario: Client mode navigates to connection setup
- **WHEN** the user selects "Client" mode
- **THEN** the app navigates to the WiFi connection setup page with client options pre-selected
