## Why

Several pages in the app lack ScrollView wrappers or have auto-hiding scrollbars, making content unreachable when it overflows the viewport. Specifically: SourceSelectionPage and DestinationSelectionPage have no ScrollView at all, so folders/buttons can be cut off. HomePage has a ScrollView but Windows hides its scrollbar by default.

## What Changes

- Add `<ScrollView>` wrapper around SourceSelectionPage content so folder lists and action buttons are scrollable
- Add `<ScrollView>` wrapper around DestinationSelectionPage content for the same reason
- Configure ScrollView on all pages to always show a vertical scrollbar on Windows via MAUI handler customization in `MauiProgram.cs`
- Preserve the existing layout and behavior on iOS/Android/MacCatalyst (native scrollbars already appear appropriately)

## Capabilities

### New Capabilities
- `scrollbar-main-screen`: Always-visible vertical scrollbar across all scrollable pages via platform handler configuration

### Modified Capabilities
- *(none - no spec-level behavior changes)*

## Impact

- `file-mover-ui/Pages/SourceSelectionPage.xaml` - wrap content in ScrollView
- `file-mover-ui/Pages/DestinationSelectionPage.xaml` - wrap content in ScrollView
- `file-mover-ui/MauiProgram.cs` - add Windows platform handler configuration for ScrollView (already done)
