## Context

Several pages in the FileMover MAUI app lack ScrollView wrappers, causing content to overflow the viewport unreachably:

- **SourceSelectionPage.xaml** — Root `<VerticalStackLayout>` with no ScrollView. When many storage providers or locations exist, folders are cut off and the Next/Cancel buttons are hidden.
- **DestinationSelectionPage.xaml** — Same layout pattern, same problem.
- **HomePage.xaml, TransferModePage.xaml, ReviewPage.xaml** — Already have `<ScrollView>` wrappers, but on Windows the scrollbar auto-hides.

On iOS, Android, and MacCatalyst, native scrollbar behavior is already appropriate and should not be changed.

## Goals / Non-Goals

**Goals:**
- Add ScrollView wrappers to SourceSelectionPage and DestinationSelectionPage so all content is reachable
- Vertical scrollbar is always visible on all scrollable pages when running on Windows
- No behavioral changes on iOS, Android, or MacCatalyst

**Non-Goals:**
- Not changing page layouts beyond adding ScrollView wrappers
- Not adding custom scrollbar styling or theming
- Not modifying FileSelectionPage (uses Grid with self-scrolling CollectionView)

## Decisions

| Decision | Rationale |
|---|---|
| **Use MAUI handler customization** in `MauiProgram.cs` | The cleanest approach to configure platform-specific behavior without touching XAML. MAUI's `ConfigureMauiHandlers` API allows modifying the native `ScrollViewer` properties per platform. |
| **Target only Windows** via `#if WINDOWS` conditional | Avoids accidentally affecting other platforms where scrollbar behavior is already acceptable. |
| **Set `ScrollViewer.VerticalScrollBarVisibility` to `Visible`** | The standard Windows attached property that forces the vertical scrollbar to always render. Alternative `Auto` would keep default auto-hide behavior. |

**Alternatives considered:**
- Adding `VerticalScrollBarVisibility` as a XAML attached property: Not supported in MAUI XAML directly for Windows-specific properties.
- Custom `ScrollView` subclass: Unnecessary complexity for a single-property change.

## Risks / Trade-offs

- **[Minor visual change]** The always-visible scrollbar takes ~16px of horizontal space. The HomePage layout already has `Padding="20"` and `Spacing="15"`, so this is negligible.
- **[MAUI version compatibility]** The handler API surface may change across MAUI versions. Using `Microsoft.Maui.Handlers.ScrollViewHandler` which has been stable since .NET MAUI GA.
