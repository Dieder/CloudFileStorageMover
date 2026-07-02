## Why

Users need to configure their cloud provider credentials (API keys, OAuth secrets) from within the app. Currently there is no UI for this — credentials must be manually edited in config files. This change adds a settings/setup screen where users can enter and manage credentials for Google Drive and Amazon Prime providers.

## What Changes

- Add a new `CredentialsConfigPage` accessible from a Shell toolbar item at the top of the screen
- Add `ICredentialsService` interface and implementation for secure credential storage
- Add `CredentialsConfigViewModel` with per-provider credential fields
- Add navigation route `credentials-config` registered in `AppShell`
- Store credentials in platform-secure storage (e.g., `SecureStorage` on mobile, encrypted file on desktop)
- Add Shell `ToolbarItem` across key pages (home, source/destination selection, file selection) that opens the config screen
- Update `SourceSelectionPage` to show `IsConfigured` status per provider with a "Configure" button for unconfigured providers

## Capabilities

### New Capabilities
- `credentials-config-ui`: UI page and ViewModel for entering/storing provider credentials (Google Drive OAuth2, Amazon Prime API key/secret)
- `credentials-service`: Service for secure credential storage and retrieval per provider

### Modified Capabilities
- `file-mover-ui`: Add requirement for provider configuration UI and credential-aware provider status

## Impact

- `Pages/CredentialsConfigPage.xaml` — new XAML page
- `Pages/CredentialsConfigPage.xaml.cs` — new code-behind
- `ViewModels/CredentialsConfigViewModel.cs` — new ViewModel
- `Services/ICredentialsService.cs` — new service interface
- `Services/CredentialsService.cs` — new service implementation (SecureStorage/encrypted file)
- `AppShell.xaml` — add ToolbarItem with gear icon that navigates to credentials config
- `AppShell.xaml.cs` — register `credentials-config` route and toolbar click handler
- `MauiProgram.cs` — register new service, ViewModel, and page in DI
- `Pages/SourceSelectionPage.xaml` — show configure button for unconfigured providers
- `ViewModels/SourceSelectionViewModel.cs` — expose IsConfigured state per provider
