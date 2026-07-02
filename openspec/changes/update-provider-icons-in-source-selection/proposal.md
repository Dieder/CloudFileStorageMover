## Why

The source selection screen currently shows platform-agnostic icons for cloud storage providers (generic cloud icon, Android logo for Google Drive). This creates visual inconsistency and confusion because:

- Users expect platform-specific icons matching what they actually use (Google Drive's colored "G" or Amazon S3 logo)
- The Android logo for Google Drive is misleading on non-Android platforms
- Providers like Prime and Windows use "folder" icons which don't distinguish them from local folders

A change to provider-specific icons improves user experience by making the UI more intuitive and accurately representing available storage services.

## What Changes

- Update provider selection icons to use provider-specific visual identifiers:
  - Google Drive: colored "G" or Google logo-style icon
  - Amazon S3: Amazon AWS logo or S3-specific icon
  - Azure Blob Storage: Azure logo or blob storage icon
  - Google Cloud Storage: Google logo or GCS icon
  - Windows: windows logo icon (instead of generic folder)
  - Prime: provider-specific icon (clarify what "prime" refers to)

## Capabilities

### New Capabilities
- `provider-specific-icons`: Display platform-appropriate icons for each storage provider in the source selection screen

### Modified Capabilities
- (none)

## Impact

- `ViewModels/SourceSelectionViewModel.cs` — Add icon property definitions for each provider
- `Pages/SourceSelectionPage.xaml.cs` — Map provider types to icon resources
- UI files (`.xaml`): Update image sources to use provider-specific icons
- Icon assets need to be added to appropriate asset directories