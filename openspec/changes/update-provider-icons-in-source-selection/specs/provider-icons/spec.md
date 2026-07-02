## ADDED Requirements

### Requirement: Provider selection screen uses provider-specific icons

When the user opens the source selection screen, the UI SHALL display provider-specific icons that visually represent each storage service:

#### Scenario: Display Google Drive with appropriate icon
- **WHEN** user opens source selection screen
- **THEN** Google Drive provider SHALL display a Google-style icon (colored 'G' or Google logo)

#### Scenario: Display Amazon S3 with appropriate icon
- **WHEN** user opens source selection screen
- **THEN** Amazon S3 provider SHALL display an AWS-specific icon (AWS logo or S3 stylized)

#### Scenario: Display Azure Blob Storage with appropriate icon
- **WHEN** user opens source selection screen
- **THEN** Azure Blob Storage provider SHALL display an Azure logo icon

#### Scenario: Display Google Cloud Storage with appropriate icon
- **WHEN** user opens source selection screen
- **THEN** Google Cloud Storage provider SHALL display a Google-style icon (consistent with Google Drive)

#### Scenario: Display Windows local storage with appropriate icon
- **WHEN** user opens source selection screen
- **THEN** Windows provider SHALL display the Windows logo icon (instead of generic folder)

#### Scenario: Display Prime storage with appropriate icon
- **WHEN** user opens source selection screen
- **THEN** Prime provider SHALL display a platform-specific icon representing the Prime service

### Requirement: Provider icons use provider type as identifier

The system SHALL map each provider's `Type` field (e.g., "s3", "azure", "gcs", "local", "prime") to an appropriate icon resource.

#### Scenario: Map provider types to icon resources
- **WHEN** creating the source selection items
- **THEN** the UI SHALL use the provider's `Type` field to select the correct icon resource from the icon mapping

### Requirement: Provider-specific icons have consistent styling

Provider icons SHALL use a consistent size, aspect ratio, and visual style across all items in the source selection list.

#### Scenario: Display icons in a consistent layout
- **WHEN** items are rendered in the collection view
- **THEN** all icons SHALL:
  - Have the same width and height (e.g., 32x32 pixels)
  - Be displayed inside the same container pattern
  - Use a consistent fill color (e.g., dark blue or primary app color)

### Requirement: Platform-native icons are used on their platforms

On Windows, provider icons SHALL use platform-native visual elements where available.

#### Scenario: Use Windows logo for Windows provider
- **WHEN** displaying the Windows provider
- **THEN** the UI SHALL use the Windows logo icon instead of a generic folder icon to represent Windows local storage

### Requirement: Fallback icons are used for unknown providers

If a provider type is not recognized, the system SHALL use a fallback icon (e.g., generic cloud or information icon).

#### Scenario: Fallback for unknown provider type
- **WHEN** a provider has a Type not recognized in the icon mapping
- **THEN** the UI SHALL display a fallback icon with a grayed-out appearance