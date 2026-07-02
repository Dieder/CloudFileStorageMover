## 1. System Folder Injection in ViewModel

- [ ] 1.1 Add `IsDesktopLocation` detection method in `FileSelectionViewModel` that compares `CurrentPath` against `Environment.GetFolderPath(SpecialFolder.Desktop)`
- [ ] 1.2 Add a static mapping of system folder names to their `Environment.SpecialFolder` values (Downloads, Documents, Pictures, Videos, Music)
- [ ] 1.3 Update `LoadFromFileSystem` to inject system folder entries at the top of `AllFiles` when the current path is the Desktop location
- [ ] 1.4 Guard system folder injection with `Directory.Exists()` to skip missing folders

## 2. Navigation Integration

- [ ] 2.1 Verify `NavigateToFolder` works with system folder entries (they use real filesystem paths)
- [ ] 2.2 Ensure system folder entries are not injected when navigating into subfolders (only on Desktop root)

## 3. Build & Verify

- [ ] 3.1 Build project with 0 errors
- [ ] 3.2 Verify: selecting Desktop shows Downloads, Documents, Pictures, Videos, Music at the top of the file list
- [ ] 3.3 Verify: clicking a system folder navigates into its real directory
- [ ] 3.4 Verify: navigating into a subfolder from Desktop root no longer shows system folder entries
- [ ] 3.5 Verify: non-Desktop locations (Downloads, Documents) are unaffected
