## 1. Icon Mapping and Resources

- [ ] 1.1 Add icon mapping dictionary in a suitable location (could be a constants file or extension method)
- [ ] 1.2 Define icon resources for each provider type (Google Drive, Amazon S3, Azure, Google Cloud, Windows, Prime)
- [ ] 1.3 Add icon resources to project assets (e.g., in MAUI .NET 8+ support Icons collection)

## 2. Update Source Selection ViewModel

- [ ] 2.1 Add `IconResource` property to `StorageProvider` model
- [ ] 2.2 Update `SourceSelectionViewModel` to map provider `Type` to icon resource using the icon mapping
- [ ] 2.3 Update `LoadProvidersAsync` to set icon resources for each provider

## 3. Update UI Rendering

- [ ] 3.1 Update `SourceSelectionPage.xaml` to display provider-specific icons instead of generic folder icons
- [ ] 3.2 Add icon binding in the `CollectionView.ItemTemplate` to use the provider's `IconResource`
- [ ] 3.3 Ensure icons are sized consistently across platforms
- [ ] 3.4 Add fallback icon handling for unknown provider types

## 4. Build and Verify

- [ ] 4.1 Build project with 0 errors
- [ ] 4.2 Verify that provider selection screen now shows appropriate platform-specific icons
- [ ] 4.3 Verify that icons look consistent with the platform
- [ ] 4.4 Verify that fallback icon is displayed when needed