## MODIFIED Requirements

### Requirement: File mover UI provides source storage selection
The system SHALL allow users to choose a source cloud storage provider or location from a list of available sources.

#### Scenario: Display available source locations
- **WHEN** the user opens the file mover UI
- **THEN** the UI displays the configured source storage providers and available connection endpoints

#### Scenario: Refresh provider status after credentials config
- **WHEN** the user returns from the credentials config page
- **THEN** the source selection view refreshes provider configuration status and reloads locations for the selected provider
