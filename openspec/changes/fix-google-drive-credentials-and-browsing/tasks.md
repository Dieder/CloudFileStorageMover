## 1. Credentials Config UI

- [ ] 1.1 Remove `IsPassword="True"` from the Field1 Entry in `CredentialsConfigPage.xaml` (Client ID field)
- [ ] 1.2 Keep `IsPassword="True"` on the Field2 Entry (Client Secret field remains masked)

## 2. GoogleDriveStorageProvider OAuth2 Credentials

- [ ] 2.1 Inject `ICredentialsService` into `GoogleDriveStorageProvider` constructor
- [ ] 2.2 Update `GetDriveServiceAsync` to construct `ClientSecrets` from saved ClientId/ClientSecret instead of loading `client_secrets.json`
- [ ] 2.3 Add fallback: if no saved credentials, fall back to `client_secrets.json` file

## 3. FileMoverService Provider Resolution

- [ ] 3.1 Fix `GetProviderForLocation` to correctly resolve the provider based on the location's actual provider type instead of defaulting to "local"
- [ ] 3.2 Pass location metadata through so file listing targets the right provider

## 4. Source Selection Refresh

- [ ] 4.1 Reload provider configuration status when returning from credentials config page
- [ ] 4.2 Trigger location refresh for selected provider after credentials are saved

## 5. Registration and Build

- [ ] 5.1 Update `MauiProgram.cs` if `GoogleDriveStorageProvider` constructor signature changed
- [ ] 5.2 Build and verify 0 errors
