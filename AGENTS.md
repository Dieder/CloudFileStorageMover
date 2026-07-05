# AGENTS.md — CloudStorageMover Development Guide

## Project Identity

CloudStorageMover is a **.NET MAUI cross-platform desktop-style application** for selecting, previewing, and transferring files (videos, images, documents) between local drives and cloud storage providers (Google Drive, Meta, Prime).

**Target platforms:** Windows, macOS, (Android/iOS secondary)  
**SDK:** .NET 10, MAUI 10.0  
**UI pattern:** MVVM with CommunityToolkit.Mvvm  
**Logging:** `Microsoft.Extensions.Logging` (`ILogger<T>`) via DI  
**Testing:** Microsoft.Maui.Testing (built-in)

---

## Application Purpose & Goals

### Purpose

Select files and folders from local drives or cloud storage — preview, edit, and transfer them to another destination (desktop, external hard drive, cloud).

### Core Goals

**Source Management**
- Default source folders: Downloads, Images, Videos, Documents (platform common folders)
- Users can add custom folders per source in configuration
- Sources: Local drives, Google Drive, Meta, Prime
- Example source data at `C:\repos\CloudStorageMover\examples\`

**File Selection & Transfer**
- Select individual files or entire folders
- Copy or move files to a destination (desktop, external hard drive, or another cloud)
- Cloud files must be downloaded locally before editing

**Preview & Metadata**
- File selected → show preview + metadata in right panel
- Photo: thumbnail preview + EXIF metadata
- Video: thumbnail + embedded player + duration
- Documents: icon/thumbnail + file metadata (size, date, type)

**Image Editing**
- Image cropping, rotation, filter application
- Editable parameters in the right panel
- Support for RAW image formats
- Image resizing and format conversion

---

## Screen Layout

```
┌──────────┬──────────┬────────────────────────┬──────────────────────┐
│ Left      │ Folders  │ Main Files & Folders   │ Right Preview/Editor │
│ Menu      │ Panel    │                        │                      │
│ (Icons)   │          │                        │                      │
│           │          │                        │                      │
│  ☰        │ Folder1  │ 📁 Subfolder1         │ [Preview Thumbnail]  │
│  🖼       │ Folder2  │ 📁 Subfolder2         │                      │
│  🎬       │ Folder3  │ 🖼 image.jpg          │ Metadata:            │
│  ☁️       │  ...     │ 🎬 video.mp4         │ - EXIF / Duration    │
│  ⚙️       │          │ 📄 document.pdf      │ - Size, Date, Type   │
│           │          │  ...                  │                      │
│           │          │                        │ [Editor Parameters]  │
│           │          │                        │ - Crop, Rotate       │
│           │          │                        │ - Filters            │
│           │          │                        │ - Resize, Convert    │
└──────────┴──────────┴────────────────────────┴──────────────────────┘
```

5-zone layout:
1. **Left Menu** — icon-only navigation (sources, settings)
2. **Folders Panel** — tree/folder list for selected source
3. **Main Files Panel** — file/folder grid for selected folder
4. **Right Preview** — thumbnail, metadata, player
5. **Right Editor** — parameter controls (visible when editing an image)

---

## Agent Hierarchy

- **Orchestrator** (main agent): Plans changes via OpenSpec artifacts (proposal → design → tasks). Delegates implementation to subagents. Reviews output. Never writes code directly.
- **Subagents** (implementation only):
  - `maui-dev` — General MAUI development
  - `maui-ui-dev` — UI with CommunityToolkit.Mvvm + Control Library
  - `maui-navigation-dev` — Shell navigation, route registration, page flow
  - `maui-styling-dev` — Resource dictionaries, themes, desktop styling
  - `ui-review` — XAML/C# code review for standards compliance

---

## Project Structure

```
src/
├── CloudStorageMover.App/           # Main MAUI app
│   ├── Pages/                       # Shell pages (5-zone layout screens)
│   ├── ViewModels/                  # CommunityToolkit.Mvvm ViewModels
│   ├── Services/                    # App-level services
│   └── Resources/                   # App-specific resources
│
├── CloudStorageMover.Controls/      # Reusable UI control library
│   ├── Controls/                    # Custom controls (FileGrid, PreviewPane, etc.)
│   ├── Converters/                  # Value converters
│   ├── Templates/                   # Data templates
│   └── Resources/                   # Shared resources
│
└── CloudStorageMover.Core/          # Abstractions & interfaces
    ├── Interfaces/                  # IStorageProviderService, etc.
    ├── Models/                      # Domain models
    └── Services/                    # Core service interfaces
```

**Control Library rules:**
- NO business logic, NO ViewModels, NO service references
- Controls use `BindableProperty` only for public API
- Never reference ViewModel types

---

## Architecture Pillars

- **MVVM** with CommunityToolkit.Mvvm (`[ObservableObject]`, `[ObservableProperty]`, `[RelayCommand]`)
- **Shell navigation** with GoToAsync, flat kebab-case routes
- **DI** via `MauiProgram.cs` — register services, ViewModels, pages as Transient/Singleton
- **Logging** via `Microsoft.Extensions.Logging`, `ILogger<T>` injected into all services and ViewModels
- **Desktop-style UI** — wider layouts, consistent spacing, keyboard navigation, resizable window (1200x800+ default)

---

## Logging

- `Microsoft.Extensions.Logging` with `ILogger<T>` injected via DI
- `MauiProgram.cs` configures logging with `builder.Logging.AddDebug()` in DEBUG
- Log levels: Information (normal flow), Warning (anomalies), Error (failures)
- All services and ViewModels receive `ILogger<T>` through constructor injection

---

## Testing

- Microsoft.Maui.Testing (built-in) for unit tests
- Test ViewModels, Services, and Core logic
- UI controls tested via the running app

---

## Development Workflow

1. **Orchestrator** creates an OpenSpec change: Proposal → Design → Tasks
2. **Orchestrator selects task** and delegates to the appropriate subagent
3. **Subagent implements** one task at a time — reads spec/design context first
4. **Orchestrator reviews** output, marks task complete
5. Repeat steps 2-4 until all tasks done
6. **Orchestrator archives** the change

**Rules:**
- One task at a time — no multitasking across changes
- Controls go in `CloudStorageMover.Controls`, never in App
- Logging (`ILogger<T>`) must be added to every new service and ViewModel
- If task is ambiguous, pause and ask user — never guess
- Solution file: `src/CloudStorageMover.slnx`

---

## Coding Conventions

- **XAML:** Compiled bindings with `x:DataType`, `StaticResource` for styles, Padding 20 on pages, Spacing 10-15 on layouts, `CollectionView` over `ListView`, `Border` with `StrokeShape="RoundRectangle CornerRadius=8"` over `Frame`
- **C#:** File-scoped namespaces, nullable reference types enabled, `_camelCase` for private fields, `PascalCase` for properties/methods, `async Task` over `async void`
- **Colors:** Primary `#512BD4`, all colors in `Colors.xaml` with matching `Brush` resources, `AppThemeBinding` for light/dark
- **Styles:** Implicit styles for standard controls, named styles (`PrimaryButton`, `Headline`, etc.) for semantic variants — all in `Styles.xaml`

---

## Desktop Styling Specifics

- Application window: wider default size (1200x800+), resizable
- Left menu: fixed-width icon bar (50-60px), dark background
- Folder panel: 200-250px wide, tree view with expand/collapse
- Main panel: flexible width, grid/list toggle
- Right panels: 300-350px wide for preview + editor
- All panels: separators between zones, headers per zone
- Desktop-appropriate font sizes (14px body minimum)
