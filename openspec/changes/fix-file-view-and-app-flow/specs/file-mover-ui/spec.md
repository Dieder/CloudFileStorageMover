## MODIFIED Requirements

### Requirement: File mover UI shows file type filter toolbar
The system SHALL provide a filter toolbar in the file selection view with buttons for "All", "Images", "Documents", and "Videos", where each filter operates on the full file set.

#### Scenario: Filter by images
- **WHEN** the user clicks the "Images" filter button
- **THEN** the file list shows only files with image extensions (jpg, jpeg, png, gif, bmp, webp)

#### Scenario: Filter by documents
- **WHEN** the user clicks the "Documents" filter button
- **THEN** the file list shows only files with document extensions (pdf, doc, docx, xls, xlsx, ppt, pptx, txt, csv)

#### Scenario: Filter by videos
- **WHEN** the user clicks the "Videos" filter button
- **THEN** the file list shows only files with video extensions (mp4, mov, avi, mkv, wmv, m4v, flv, rm, rmvb)

#### Scenario: Show all files after filter
- **WHEN** the user clicks the "All" filter button after applying any filter
- **THEN** the file list restores to show all files and folders

#### Scenario: Active filter button is highlighted
- **WHEN** a filter is active
- **THEN** the active filter button has a filled background and white text, while inactive buttons have a transparent background with a colored border

#### Scenario: Sequential filters operate from full set
- **WHEN** the user clicks "Images" and then "Documents"
- **THEN** the file list shows all document files (not filtered from the previous image set)
