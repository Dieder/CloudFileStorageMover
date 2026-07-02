## 1. Platform Handler Configuration (Done)

- [x] 1.1 Add `using Microsoft.Maui.Handlers;` and `using Microsoft.UI.Xaml.Controls;` to MauiProgram.cs (with `#if WINDOWS` guard)
- [x] 1.2 Register ScrollView handler customization in `MauiProgram.cs` via `ConfigureMauiHandlers` that sets `ScrollViewer.VerticalScrollBarVisibility` to `Visible` on Windows

## 2. Add ScrollView Wrappers to Selection Pages

- [x] 2.1 Add `<ScrollView>` wrapper around the root `<VerticalStackLayout>` in `SourceSelectionPage.xaml`
- [x] 2.2 Add `<ScrollView>` wrapper around the root `<VerticalStackLayout>` in `DestinationSelectionPage.xaml`

## 3. Verification

- [x] 3.1 Build the project for Windows target (`net10.0-windows10.0.19041.0`) and confirm no compilation errors
- [ ] 3.2 Run the app and verify all transfer flow pages have reachable content and visible scrollbar on Windows
