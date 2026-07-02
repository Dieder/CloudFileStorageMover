## 1. Credentials Service

- [x] 1.1 Create `Services/ICredentialsService.cs` interface with `SaveCredentialsAsync`, `GetCredentialsAsync`, `ClearCredentialsAsync`, `IsConfiguredAsync`
- [x] 1.2 Create `Services/CredentialsService.cs` implementation using `SecureStorage` with encrypted file fallback
- [x] 1.3 Register `ICredentialsService` and `CredentialsService` in DI via `MauiProgram.cs`

## 2. Credentials Config Page

- [x] 2.1 Create `Pages/CredentialsConfigPage.xaml` with provider list and credential entry form
- [x] 2.2 Create `Pages/CredentialsConfigPage.xaml.cs` code-behind with navigation and binding setup
- [x] 2.3 Create `ViewModels/CredentialsConfigViewModel.cs` with per-provider fields and save/clear commands
- [x] 2.4 Register `credentials-config` route in `AppShell.xaml.cs`
- [x] 2.5 Register `CredentialsConfigPage` and `CredentialsConfigViewModel` in DI via `MauiProgram.cs`

## 3. Shell Toolbar Item

- [x] 3.1 Add `ToolbarItem` with gear icon to key pages (home, source selection, destination selection, file selection)
- [x] 3.2 Add toolbar click handler that navigates to `credentials-config` page

## 4. Source Selection Provider Status

- [x] 4.1 Update `SourceSelectionViewModel.cs` to load `IsConfigured` status per provider via `ICredentialsService`
- [x] 4.2 Update `SourceSelectionPage.xaml` to show configured/unconfigured badge per provider
- [x] 4.3 Add "Configure" button for unconfigured providers that navigates to `credentials-config`
