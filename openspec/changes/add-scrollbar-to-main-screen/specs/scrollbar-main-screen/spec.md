## ADDED Requirements

### Requirement: Source selection page content is scrollable with visible scrollbar on Windows

The system SHALL wrap the source selection page content in a ScrollView with an always-visible vertical scrollbar on Windows.

#### Scenario: Scroll folders list on source selection
- **WHEN** the user opens the source selection page with more providers/locations than fit the viewport
- **THEN** the user can scroll to see all items and the action buttons at the bottom

#### Scenario: Scrollbar visible on source selection page on Windows
- **WHEN** the user opens the source selection page on Windows
- **THEN** the vertical scrollbar is visibly rendered alongside the scrollable content

### Requirement: Destination selection page content is scrollable with visible scrollbar on Windows

The system SHALL wrap the destination selection page content in a ScrollView with an always-visible vertical scrollbar on Windows.

#### Scenario: Scroll locations list on destination selection
- **WHEN** the user opens the destination selection page with more providers/locations than fit the viewport
- **THEN** the user can scroll to see all items and the action buttons at the bottom

### Requirement: Home screen shows an always-visible vertical scrollbar on Windows

The system SHALL display an always-visible vertical scrollbar on the home screen when running on Windows.

#### Scenario: Scrollbar visible on Windows startup
- **WHEN** the user opens the app on Windows and views the home screen
- **THEN** the vertical scrollbar is visibly rendered alongside the scrollable content

#### Scenario: Default scrollbar behavior on non-Windows platforms
- **WHEN** the user opens the app on iOS, Android, or MacCatalyst
- **THEN** the scrollbar uses the platform-default behavior (auto-hide on iOS/Android, system default on MacCatalyst)
