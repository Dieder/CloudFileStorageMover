## Context

Currently, the source selection screen shows inconsistent icons for cloud storage providers:
- Google Drive displays a generic Android logo (inappropriate for Android)
- Amazon S3, Azure Blob Storage, Google Cloud Storage, and other providers use generic folder icons
- All cloud storage providers look identical despite being different services
- Windows local storage uses a generic folder icon instead of the Windows logo

This creates visual confusion for users when choosing between storage providers.

## Goals / Non-Goals

**Goals:**
- Display platform-appropriate icons for each storage provider:
  - Google Drive/Google Cloud Storage: Google-style icon (colored "G" or Google logo)
  - Amazon S3: AWS logo or S3-specific icon
  - Azure Blob Storage: Azure logo or blob storage icon
  - Google Cloud Storage: Google logo (consistent with Google Drive)
  - Windows local storage: Windows logo icon
  - Prime: Prime-specific icon
- Maintain UI consistency across platforms with appropriate native icons
- Map provider `Type` field to icon resources automatically
- Keep UI changes minimal and focused on icon display only

**Non-Goals:**
- Changing provider selection logic or functionality
- Adding new storage providers beyond existing ones
- Changing icon sizes or layout beyond visual enhancement
- Adding animations or interactive effects to icons

## Decisions

- **Decision**: Use the provider's `Type` field to determine icon selection
  - **Rationale**: Each provider (AWS, Azure, Google Cloud, etc.) has a distinct type identifier that maps cleanly to icon assets
  - **Alternative considered**: Hard-coding icon names per provider type — cleaner and maintainable

- **Decision**: Map provider types to icon resources via a static dictionary
  - **Rationale**: Simple O(1) lookup for icon selection, no external dependencies needed
  - **Alternative considered**: Loading icons from resources folder with filename-based lookup — more flexible but adds complexity

- **Decision**: Use platform-native icons where available (Windows logo for Windows provider)
  - **Rationale**: Better user recognition on Windows platform
  - **Alternative considered**: Using corporate-style icons (e.g., AWS logo for S3) — clear but may not be native to platform

## Risks / Trade-offs

- [Icon resource management] → Need to add icon assets to project resources; ensure proper license compliance
- [Icon mapping accuracy] → Provider types may change; icon mapping table needs maintenance
- [Platform consistency] → Default light/dark mode icons may need separate assets
- [Fallback handling] → Unknown provider types need fallback icon to prevent UI breakage
- [Design across platforms] → Android, iOS, and Desktop icon styles should be consistent but platform-appropriate