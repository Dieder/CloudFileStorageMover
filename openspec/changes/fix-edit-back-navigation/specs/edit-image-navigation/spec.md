## ADDED Requirements

### Requirement: Cancel from image editor returns to source selection

When the user cancels from the ImageEditorPage, the system SHALL navigate back to the SourceSelectionPage, bypassing the FileSelectionPage.

#### Scenario: Cancel from image editor after opening from file selection
- **WHEN** user is on FileSelectionPage and clicks "Edit Image"
- **AND** the ImageEditorPage appears
- **AND** user clicks "Cancel"
- **THEN** the system navigates to SourceSelectionPage

#### Scenario: Hardware back button on image editor
- **WHEN** user is on ImageEditorPage
- **AND** user presses the hardware/software back button
- **THEN** the system navigates to SourceSelectionPage

### Requirement: Save from image editor returns to source selection

When the user saves an edited image from the ImageEditorPage, the system SHALL navigate back to the SourceSelectionPage with a confirmation that the image was saved.

#### Scenario: Save edited image
- **WHEN** user clicks "Save Copy" on ImageEditorPage
- **AND** the save succeeds
- **THEN** the system navigates to SourceSelectionPage

### Requirement: File selection back button returns to source selection

The "Back" button on FileSelectionPage SHALL always navigate to SourceSelectionPage, regardless of prior navigation history (e.g., after returning from ImageEditorPage).

#### Scenario: Back from file selection after image editor visit
- **WHEN** user navigates: SourceSelection → FileSelection → ImageEditor → (Cancel) → FileSelection
- **AND** user clicks "Back" on FileSelectionPage
- **THEN** the system navigates to SourceSelectionPage
