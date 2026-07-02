## ADDED Requirements

### Requirement: Local device shows standard user folders

When "Local Device" is selected as source, the system SHALL display common Windows user folders (Downloads, Documents, Pictures, Videos, Music, Desktop) as selectable locations.

#### Scenario: View local folders
- **WHEN** the user selects "Local Device" as the storage provider
- **THEN** the locations list includes Downloads, Documents, Pictures, Videos, Music, and Desktop

### Requirement: Folder drill-down navigation

The system SHALL allow users to tap a folder in the file list to navigate into it and see its contents.

#### Scenario: Navigate into subfolder
- **WHEN** the user taps a folder item in the file list
- **THEN** the file list loads and displays the contents of that folder

#### Scenario: Navigate back from subfolder
- **WHEN** the user is viewing a subfolder's contents
- **THEN** the user can navigate back to the parent folder

### Requirement: Source routing for local folders

When the user selects a local device folder and taps Next, the system SHALL navigate to the file selection page to browse the folder contents, not directly to destination selection.

#### Scenario: Local source goes to file browsing
- **WHEN** the user selects a local storage provider and a folder location, then taps Next
- **THEN** the app navigates to the file selection page with the selected folder's files loaded

#### Scenario: Cloud source continues to destination
- **WHEN** the user selects a cloud storage provider and a location, then taps Next
- **THEN** the app continues to destination selection as before

### Requirement: Filter toolbar shows active state

The filter toolbar buttons (All, Images, Documents, Videos) SHALL visually indicate which filter is currently active.

#### Scenario: Active filter is highlighted
- **WHEN** the user taps the "Images" filter button
- **THEN** that button is visually highlighted (e.g., primary color background) while the other buttons return to the default inactive style
