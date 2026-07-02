## Why

The Google Drive OIDC Client ID is hidden behind a password mask in the credentials config UI, preventing users from seeing or verifying their configured value. Additionally, clicking Google Drive as a source shows no contents because the `GoogleDriveStorageProvider` requires a `client_secrets.json` file on disk rather than using credentials already saved via `ICredentialsService`.

## What Changes

- Remove `IsPassword="True"` from the Client ID Entry field so the value is visible (keep it on Client Secret)
- Update `GoogleDriveStorageProvider` to accept `ICredentialsService` and use saved ClientId/ClientSecret for OAuth2 authentication instead of requiring a `client_secrets.json` file
- Update `FileMoverService` and `SourceSelectionViewModel` to refresh provider status and locations after credentials are saved
- Wire the Google Drive config path so the provider can authenticate using credentials from the UI

## Capabilities

### New Capabilities
- `credentials-config-ui-update`: Updated credential entry field visibility for Google Drive provider configuration

### Modified Capabilities
- `google-drive-provider`: Accept credentials from SecureStorage via `ICredentialsService` for OAuth2 auth, removing hard dependency on `client_secrets.json` file
- `file-mover-ui`: Refresh provider status and Google Drive contents after credentials are configured

## Impact

- `Pages/CredentialsConfigPage.xaml` — remove `IsPassword` from Client ID entry
- `Services/GoogleDriveStorageProvider.cs` — inject `ICredentialsService`, construct OAuth2 credentials from saved ClientId/ClientSecret
- `Services/FileMoverService.cs` — pass `ICredentialsService` to providers, refresh locations properly
- `ViewModels/SourceSelectionViewModel.cs` — refresh provider list after returning from credentials config
