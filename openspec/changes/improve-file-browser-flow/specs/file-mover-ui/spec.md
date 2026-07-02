## ADDED Requirements

### Requirement: File list supports folder drill-down
The system SHALL allow users to navigate into subfolders by tapping a folder in the file list, and navigate back to the parent folder.

#### Scenario: Tap folder to view contents
- **WHEN** the user taps a folder item in the file list
- **THEN** the file list updates to show the contents of that folder

#### Scenario: Navigate back from subfolder
- **WHEN** the user is viewing a subfolder and taps a back control
- **THEN** the file list returns to the parent folder contents

### Requirement: File filter toolbar shows active state
The system SHALL visually highlight the currently active file type filter button so the user knows which filter is applied.

#### Scenario: Active filter is visually distinct
- **WHEN** the user selects the "Images" filter
- **THEN** the Images button is visually highlighted and the other buttons return to default style

#### Scenario: All filter resets the view
- **WHEN** the user selects the "All" filter
- **THEN** all files and folders in the current directory are displayed, and the All button is highlighted

### Requirement: Local source selection navigates to file browsing
When the user selects a local storage provider as the source, selecting a folder and tapping Next SHALL navigate to the file selection page to browse the folder contents.

#### Scenario: Local source goes to file selection
- **WHEN** the user selects a local device folder as the source and taps Next
- **THEN** the app navigates to the file selection page showing the folder contents

## MODIFIED Requirements

### Requirement: File mover UI allows selecting files or folders

The system SHALL allow users to browse and select files or folders from the chosen source location, including the ability to drill into subfolders, and a folder picker experience for photo and document directories.

#### Scenario: Select files to move
- **WHEN** the user navigates to a source folder
- **THEN** the UI shows the files and subfolders in that location and allows selection of one or more items

#### Scenario: Drill into subfolder via tap
- **WHEN** the user taps a folder in the file list
- **THEN** the UI navigates into that folder and displays its contents for further selection

#### Scenario: Use folder picker for photos or documents
- **WHEN** the user opens the folder picker and selects an image or document folder
- **THEN** the UI navigates into that folder and displays its contents
