## ADDED Requirements

### Requirement: Desktop location shows system folders as root entries

When the user selects the Desktop as the source location, the system SHALL inject well-known Windows system folders (Downloads, Documents, Pictures, Videos, Music) as navigable entries at the top of the file list. Actual Desktop contents SHALL appear below these entries.

#### Scenario: Desktop location shows system folders
- **WHEN** user selects "Desktop" as the source location
- **AND** the FileSelectionPage loads
- **THEN** the file list SHALL include "Downloads", "Documents", "Pictures", "Videos", and "Music" entries at the top
- **AND** actual Desktop files and folders SHALL appear below them

#### Scenario: System folder navigates to real path
- **WHEN** user taps on the "Downloads" system folder entry
- **THEN** the system SHALL navigate into the user's real Downloads directory and show its contents

#### Scenario: Missing system folder is omitted
- **WHEN** a system folder path does not exist on disk
- **THEN** it SHALL be silently omitted from the system folder list

### Requirement: Desktop detection uses canonical path

The system SHALL detect the Desktop location by comparing the current browsing path against the canonical `Environment.GetFolderPath(SpecialFolder.Desktop)` value.

#### Scenario: Non-Desktop locations are unaffected
- **WHEN** user selects "Downloads" as the source location
- **THEN** no system folder entries SHALL be injected
- **AND** only the actual Downloads directory contents SHALL be shown

### Requirement: Navigating away from Desktop stops showing system folders

When browsing into a subfolder (not on the Desktop root), the system SHALL show only that folder's actual contents without system folder entries.

#### Scenario: Navigate into subfolder from Desktop
- **WHEN** user taps on a regular folder inside the Desktop root
- **THEN** the system SHALL show that folder's actual contents without system folder entries
