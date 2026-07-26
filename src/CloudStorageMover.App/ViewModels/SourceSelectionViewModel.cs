using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CloudStorageMover.App.Services;
using CloudStorageMover.Core.Interfaces;
using CloudStorageMover.Core.Models;
using CloudStorageMover.Core.Services;
using CloudStorageMover.Controls.Services;
using Microsoft.Extensions.Logging;

namespace CloudStorageMover.App.ViewModels;

public partial class SourceSelectionViewModel : ObservableObject
{
    private readonly StorageProviderFactory _providerFactory;
    private readonly AppSettingsService _settings;
    private readonly SelectedFilesService _selectedFiles;
    private readonly ILogger<SourceSelectionViewModel> _logger;
    private IStorageProviderService? _selectedProvider;
    private List<ProviderItem> _allProviders = [];

    private readonly IOAuthLoopbackService _oauthLoopback;

    // Auth services
    private readonly GoogleDriveSignInService _googleDriveAuth;

    private readonly MegaSignInService _megaAuth;
    private readonly PCloudSignInService _pcloudAuth;
    private readonly GooglePhotosSignInService _googlePhotosAuth;
    private readonly DestinationSettingsService _destinationSettings;

    [ObservableProperty]
    private ProviderItem? _selectedProviderItem;

    [ObservableProperty]
    private bool _isSignedIn;

    [ObservableProperty]
    private bool _isSigningIn;

    [ObservableProperty]
    private string _signInStatus = string.Empty;

    [ObservableProperty]
    private bool _isProviderSelected;

    [ObservableProperty]
    private string _selectedProviderName = string.Empty;

    [ObservableProperty]
    private string _searchText = string.Empty;

    [ObservableProperty]
    private bool _isExpanded = true;

    [ObservableProperty]
    private string _currentPath = string.Empty;

    [ObservableProperty]
    private string _fileFilterText = string.Empty;

    [ObservableProperty]
    private bool _isBrowsingFiles;

    [ObservableProperty]
    private bool _isLoadingFiles;

    [ObservableProperty]
    private bool _isMultiSelectMode;

    [ObservableProperty]
    private int _selectedFilesCount;

    [ObservableProperty]
    private string _selectedFilesSummary = string.Empty;

    partial void OnIsBrowsingFilesChanged(bool value)
    {
        IsSourceVisible = !value;
    }

    [ObservableProperty]
    private bool _isSourceVisible = true;

    public ObservableCollection<ProviderItem> Providers { get; } = [];
    public ObservableCollection<ProviderItem> FilteredProviders { get; } = [];
    public ObservableCollection<SelectableFileItem> DisplayItems { get; } = [];

    private List<SelectableFileItem> _allDisplayItems = [];
    private readonly Stack<string> _pathStack = [];

    public IStorageProviderService? SelectedProvider => _selectedProvider;

    public event EventHandler<List<FileItem>>? FoldersLoaded;
    public event EventHandler<StorageProviderType>? ProviderChanging;

    public SourceSelectionViewModel(
        StorageProviderFactory providerFactory,
        AppSettingsService settings,
        SelectedFilesService selectedFiles,
        IOAuthLoopbackService oauthLoopback,
        GoogleDriveSignInService googleDriveAuth,

        MegaSignInService megaAuth,
        PCloudSignInService pcloudAuth,
        GooglePhotosSignInService googlePhotosAuth,
        DestinationSettingsService destinationSettings,
        ILogger<SourceSelectionViewModel> logger)
    {
        _providerFactory = providerFactory;
        _settings = settings;
        _selectedFiles = selectedFiles;
        _oauthLoopback = oauthLoopback;
        _googleDriveAuth = googleDriveAuth;

        _megaAuth = megaAuth;
        _pcloudAuth = pcloudAuth;
        _googlePhotosAuth = googlePhotosAuth;
        _destinationSettings = destinationSettings;
        _logger = logger;

        var allProviders = providerFactory.GetAllProviders();
        foreach (var p in allProviders)
        {
            var item = new ProviderItem
            {
                ProviderType = p.ProviderType,
                DisplayName = p.DisplayName
            };
            Providers.Add(item);
            _allProviders.Add(item);
        }
        ApplyFilter();
    }

    partial void OnSearchTextChanged(string value)
    {
        ApplyFilter();
    }

    private void ApplyFilter()
    {
        var text = SearchText?.Trim() ?? string.Empty;
        FilteredProviders.Clear();
        foreach (var p in _allProviders)
        {
            if (string.IsNullOrEmpty(text) || p.DisplayName.Contains(text, StringComparison.OrdinalIgnoreCase))
                FilteredProviders.Add(p);
        }
    }

    [RelayCommand]
    private void SelectSource()
    {
        SelectedProviderItem = null;
        IsProviderSelected = false;
        IsExpanded = true;
        IsBrowsingFiles = false;
        _selectedProvider = null;
        SignInStatus = string.Empty;
        DisplayItems.Clear();
        CurrentPath = string.Empty;
        IsMultiSelectMode = false;
        UpdateSelectionCount();
    }

    partial void OnSelectedProviderItemChanged(ProviderItem? value)
    {
        if (value is not null)
        {
            ProviderChanging?.Invoke(this, value.ProviderType);
            FoldersLoaded?.Invoke(this, new List<FileItem>());
            IsExpanded = false;
            _ = SelectProviderAsync(value.ProviderType);
        }
    }

    partial void OnIsProviderSelectedChanged(bool value)
    {
        if (!value)
        {
            SignInStatus = string.Empty;
        }
    }

    [RelayCommand]
    private void ToggleMultiSelect()
    {
        IsMultiSelectMode = !IsMultiSelectMode;
        if (!IsMultiSelectMode)
        {
            ClearAllSelections();
        }
    }

    [RelayCommand]
    private void SelectAllVisible()
    {
        foreach (var item in DisplayItems)
        {
            if (!item.IsDirectory && !item.IsSelected)
            {
                item.IsSelected = true;
                _selectedFiles.AddFile(item.FileItem, item.FileItem.FullPath);
            }
        }
        UpdateSelectionCount();
    }

    [RelayCommand]
    private void DeselectAll()
    {
        _selectedFiles.Clear();
        UpdateSelectionCount();
    }

    private void ClearAllSelections()
    {
        foreach (var item in DisplayItems)
            item.IsSelected = false;
        foreach (var item in _allDisplayItems)
            item.IsSelected = false;
        _selectedFiles.Clear();
        UpdateSelectionCount();
    }

    [RelayCommand]
    private void ToggleItemSelection(SelectableFileItem item)
    {
        if (item.IsDirectory)
        {
            _ = SelectFolderRecursiveAsync(item.FileItem);
        }
        else
        {
            if (item.IsSelected)
            {
                item.IsSelected = false;
                _selectedFiles.RemoveFile(item.FileItem.FullPath);
            }
            else
            {
                item.IsSelected = true;
                _selectedFiles.AddFile(item.FileItem, item.FileItem.FullPath);
            }
        }
        UpdateSelectionCount();
    }

    public async Task SelectFolderRecursiveAsync(FileItem folder)
    {
        if (_selectedProvider == null) return;

        try
        {
            IsLoadingFiles = true;
            var files = await _selectedProvider.ListFilesAsync(folder.FullPath);
            foreach (var file in files)
            {
                _selectedFiles.AddFile(file, file.FullPath);
            }

            var subFolders = await _selectedProvider.ListFoldersAsync(folder.FullPath);
            foreach (var subFolder in subFolders)
            {
                await SelectFolderRecursiveAsync(subFolder);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to recursively select folder {Path}", folder.FullPath);
        }
        finally
        {
            IsLoadingFiles = false;
            UpdateSelectionCount();
        }
    }

    private void UpdateSelectionCount()
    {
        SelectedFilesCount = _selectedFiles.Count;
        SelectedFilesSummary = _selectedFiles.Count > 0
            ? $"{_selectedFiles.Count} file(s) selected ({_selectedFiles.TotalSize / 1024.0 / 1024.0:F1} MB)"
            : "No files selected";
    }

    [RelayCommand]
    private async Task SignInAsync()
    {
        if (_selectedProvider == null) return;

        IsSigningIn = true;
        SignInStatus = "Signing in...";

        try
        {
            var success = await AuthenticateCurrentProviderAsync();
            if (success)
            {
                IsSignedIn = true;
                SignInStatus = "Signed in";
                await LoadFoldersAsync();
            }
            else
            {
                SignInStatus = "Sign-in failed. Check credentials.";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Sign-in failed");
            SignInStatus = $"Error: {ex.Message}";
        }
        finally
        {
            IsSigningIn = false;
        }
    }

    [RelayCommand]
    private async Task SignOutAsync()
    {
        _logger.LogInformation("=== SIGN-OUT SEQUENCE STARTED ===");
        
        if (_selectedProvider == null)
        {
            _logger.LogDebug("No provider selected, skipping sign-out");
            await Task.CompletedTask;
            return;
        }
        
        try
        {
            _logger.LogInformation("Starting provider sign-out for: {Provider}", 
                _selectedProvider.ProviderType);
            
            SignOutCurrentProvider();
            IsSignedIn = false;
            SignInStatus = "Signed out";
            
            _logger.LogInformation("=== PROVIDER SIGNOUT COMPLETED ===");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "=== EXCEPTION DURING PROVIDER SIGNOUT ===");
            throw;
        }
        finally
        {
            _logger.LogInformation("=== SIGN-OUT SEQUENCE COMPLETED, APP PREPARING TO CLOSE ===");
        }
        
        await Task.CompletedTask;
    }

    private async Task<bool> ExchangeGoogleCodeAsync(string label, string? clientId, string? secret, string scope, Action<string, string?, long> setTokens)
    {
        if (string.IsNullOrEmpty(clientId))
        {
            SignInStatus = $"{label} client ID not configured";
            return false;
        }

        var authUrl = "https://accounts.google.com/o/oauth2/v2/auth?" +
            $"client_id={clientId}&" +
            $"redirect_uri={Uri.EscapeDataString("http://localhost")}&" +
            $"response_type=code&" +
            $"scope={Uri.EscapeDataString(scope)}&" +
            $"access_type=offline&prompt=consent";

        _logger.LogInformation("{Label}: Starting OAuth loopback...", label);

        try
        {
            var result = await _oauthLoopback.StartAsync(authUrl);

            _logger.LogInformation("{Label}: OAuth loopback result: error={Error}, hasCode={HasCode}, redirectUri={RedirectUri}",
                label, result.Error, !string.IsNullOrEmpty(result.Code), result.RedirectUri);

            if (!string.IsNullOrEmpty(result.Error))
            {
                SignInStatus = $"{label} auth error: {result.Error}";
                _logger.LogError("{Label} auth error: {Error}", label, result.Error);
                return false;
            }

            if (string.IsNullOrEmpty(result.Code))
            {
                SignInStatus = $"{label} no authorization code received";
                return false;
            }

            _logger.LogInformation("{Label}: Exchanging code for tokens, redirect_uri={RedirectUri}", label, result.RedirectUri);

            using var httpClient = new HttpClient();
            var form = new Dictionary<string, string>
            {
                ["client_id"] = clientId,
                ["client_secret"] = secret ?? "",
                ["redirect_uri"] = result.RedirectUri ?? "http://localhost",
                ["grant_type"] = "authorization_code",
                ["code"] = result.Code
            };

            var tokenResponse = await httpClient.PostAsync(
                "https://oauth2.googleapis.com/token",
                new FormUrlEncodedContent(form));

            var json = await tokenResponse.Content.ReadAsStringAsync();

            if (!tokenResponse.IsSuccessStatusCode)
            {
                SignInStatus = $"{label} token exchange failed: {json}";
                _logger.LogError("{Label} token error: {Json}", label, json);
                return false;
            }

            using var doc = System.Text.Json.JsonDocument.Parse(json);
            var root = doc.RootElement;

            var accessToken = root.GetProperty("access_token").GetString()!;
            var refreshToken = root.TryGetProperty("refresh_token", out var rt) ? rt.GetString() : null;
            var expiresIn = root.TryGetProperty("expires_in", out var exp) ? exp.GetInt64() : 3600;

            _logger.LogInformation("{Label}: Token exchange successful, expires in {Expires}s", label, expiresIn);

            setTokens(accessToken, refreshToken, expiresIn);
            return true;
        }
        catch (OperationCanceledException)
        {
            SignInStatus = "Sign-in cancelled";
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "{Label} auth failed", label);
            SignInStatus = $"{label} auth error: {ex.Message}";
            return false;
        }
    }

    private async Task<bool> AuthenticateCurrentProviderAsync()
    {
        if (_selectedProvider == null) return false;

        switch (_selectedProvider.ProviderType)
        {
            case StorageProviderType.GoogleDrive:
                return await ExchangeGoogleCodeAsync("drive",
                    _settings.GoogleDriveClientId,
                    _settings.GoogleDriveSecret,
                    "https://www.googleapis.com/auth/drive.readonly",
                    (token, refresh, exp) => _googleDriveAuth.SetTokens(token, refresh, exp));

            case StorageProviderType.Mega:
                var megaKey = _destinationSettings.MegaKey;
                if (!string.IsNullOrEmpty(megaKey) && _megaAuth.TryRestoreFromKey(megaKey))
                    return true;

                var megaEmail = await Shell.Current.DisplayPromptAsync("MEGA Login",
                    "Email:", "Next", "Cancel",
                    placeholder: "user@example.com");
                if (string.IsNullOrEmpty(megaEmail)) return false;

                var megaPassword = await Shell.Current.DisplayPromptAsync("MEGA Login",
                    "Password:", "Sign In", "Cancel",
                    placeholder: "********");
                if (string.IsNullOrEmpty(megaPassword)) return false;

                try
                {
                    await _megaAuth.SignInAsync(megaEmail, megaPassword);
                }
                catch (OperationCanceledException)
                {
                    SignInStatus = "MEGA login timed out. Check your connection.";
                    return false;
                }
                catch (Exception ex)
                {
                    SignInStatus = $"MEGA login failed: {ex.Message}";
                    return false;
                }
                return _megaAuth.IsSignedIn;

            case StorageProviderType.PCloud:
                var pcloudUser = await Shell.Current.DisplayPromptAsync("pCloud Login",
                    "Email:", "Next", "Cancel",
                    placeholder: "user@example.com");
                if (string.IsNullOrEmpty(pcloudUser)) return false;

                var pcloudPass = await Shell.Current.DisplayPromptAsync("pCloud Login",
                    "Password:", "Sign In", "Cancel",
                    placeholder: "********");
                if (string.IsNullOrEmpty(pcloudPass)) return false;

                var (pSuccess, token) = await _pcloudAuth.AuthenticateAsync(pcloudUser, pcloudPass);
                if (pSuccess && token != null)
                {
                    _pcloudAuth.SignIn(token);
                    return true;
                }
                return false;

            case StorageProviderType.GooglePhotos:
                return await ExchangeGoogleCodeAsync("Google Photos",
                    _settings.GooglePhotosClientId,
                    _settings.GooglePhotosSecret,
                    "https://www.googleapis.com/auth/photoslibrary.readonly",
                    (token, refresh, exp) => _googlePhotosAuth.SetTokens(token, refresh, exp));

            default:
                return true;
        }
    }

    private void SignOutCurrentProvider()
    {
        if (_selectedProvider == null) return;

        switch (_selectedProvider.ProviderType)
        {
            case StorageProviderType.GoogleDrive:
                _ = _googleDriveAuth.SignOutAsync();
                break;
            case StorageProviderType.Mega:
                _ = _megaAuth.SignOutAsync();
                break;
            case StorageProviderType.PCloud:
                _pcloudAuth.SignOut();
                break;
            case StorageProviderType.GooglePhotos:
                _googlePhotosAuth.SignOut();
                break;
        }
    }

    private bool IsCurrentProviderSignedIn()
    {
        if (_selectedProvider == null) return true;

        return _selectedProvider.ProviderType switch
        {
            StorageProviderType.Desktop => true,
            StorageProviderType.GoogleDrive => _googleDriveAuth.IsSignedIn,
            StorageProviderType.Mega => _megaAuth.IsSignedIn,
            StorageProviderType.PCloud => _pcloudAuth.IsSignedIn,
            StorageProviderType.GooglePhotos => _googlePhotosAuth.IsSignedIn || _googlePhotosAuth.HasRefreshToken,
            _ => true
        };
    }

    private async Task SelectProviderAsync(StorageProviderType type)
    {
        _selectedProvider = _providerFactory.GetProvider(type);
        SelectedProviderName = _selectedProvider.DisplayName;
        IsProviderSelected = true;

        if (type == StorageProviderType.GooglePhotos)
        {
            var refreshed = await _googlePhotosAuth.EnsureTokenAsync(
                _settings.GooglePhotosClientId, _settings.GooglePhotosSecret);
            if (!refreshed)
                _logger.LogWarning("Google Photos token could not be refreshed");
        }

        IsSignedIn = IsCurrentProviderSignedIn();
        SignInStatus = IsSignedIn ? "Signed in" : "Sign in required";

        // Always show the file explorer panel after selecting a provider
        IsBrowsingFiles = true;

        if (IsSignedIn)
        {
            await LoadFoldersAsync();
        }
    }

    private async Task LoadFoldersAsync()
    {
        _logger.LogInformation("=== LOADING FOLDERS FOR PROVIDER: {Provider} ===", 
            _selectedProvider?.ProviderType ?? StorageProviderType.Desktop);
            
        if (_selectedProvider == null)
        {
            _logger.LogWarning("No provider selected, cannot load folders");
            return;
        }

        try
        {
            _logger.LogInformation("Calling GetCommonDirectoriesAsync for {Provider}", 
                _selectedProvider.ProviderType);
                
            var commonDirs = await _selectedProvider.GetCommonDirectoriesAsync();
            
            _logger.LogInformation("Retrieved {Count} common directories from {Provider}", 
                commonDirs.Count, _selectedProvider.ProviderType);
            
            var folderItems = commonDirs.Select(d =>
            {
                var parts = d.Split(['|'], 2);
                var name = parts.Length > 1 ? parts[1] : Path.GetFileName(parts[0]) ?? parts[0];
                var path = parts[0];
                return new FileItem
                {
                    Name = name,
                    FullPath = path,
                    IsDirectory = true,
                    ProviderType = _selectedProvider.ProviderType
                };
            }).ToList();

            _logger.LogInformation("Converted {Count} directories to FileItem objects", folderItems.Count);

            _pathStack.Clear();
            CurrentPath = _selectedProvider.DisplayName;
            _allDisplayItems = folderItems.Select(f => new SelectableFileItem(f)).ToList();
            IsBrowsingFiles = true;
            ApplyFileFilter();
            FoldersLoaded?.Invoke(this, folderItems);
            _logger.LogInformation("=== FOLDERS LOADED SUCCESSFULLY ===");
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP request exception loading folders for {Provider}", 
                _selectedProvider.ProviderType);
            SignInStatus = $"API error: {(int)ex.StatusCode.GetValueOrDefault()} {ex.Message}";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Generic exception loading folders for {Provider}", 
                _selectedProvider.ProviderType);
            SignInStatus = $"Error loading folders: {ex.Message}";
        }
        finally
        {
            _logger.LogInformation("=== LOAD FOLDERS OPERATION COMPLETED ===");
        }
    }

    [RelayCommand]
    private async Task OpenFolderAsync(FileItem folder)
    {
        if (_selectedProvider == null || folder == null) return;

        IsLoadingFiles = true;
        try
        {
            _pathStack.Push(folder.FullPath);
            CurrentPath = string.Join(" / ", _pathStack.Reverse());

            var subFolders = await _selectedProvider.ListFoldersAsync(folder.FullPath);
            var files = await _selectedProvider.ListFilesAsync(folder.FullPath);

            var items = new List<FileItem>();
            items.AddRange(subFolders);
            items.AddRange(files);

            _allDisplayItems = items.Select(f => new SelectableFileItem(f)).ToList();
            ApplyFileFilter();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to open folder {Path}", folder.FullPath);
            SignInStatus = $"Error: {ex.Message}";
        }
        finally
        {
            IsLoadingFiles = false;
        }
    }

    [RelayCommand]
    private void NavigateBack()
    {
        if (_pathStack.Count == 0) return;

        _pathStack.Pop();
        CurrentPath = _pathStack.Count > 0
            ? string.Join(" / ", _pathStack.Reverse())
            : _selectedProvider?.DisplayName ?? string.Empty;

        if (_pathStack.Count == 0)
        {
            _ = LoadFoldersAsync();
        }
        else
        {
            var parentPath = _pathStack.Peek();
            _ = OpenFolderAsync(new FileItem { FullPath = parentPath, IsDirectory = true });
        }
    }

    partial void OnFileFilterTextChanged(string value)
    {
        ApplyFileFilter();
    }

    private void ApplyFileFilter()
    {
        var text = FileFilterText?.Trim() ?? string.Empty;
        DisplayItems.Clear();
        foreach (var item in _allDisplayItems)
        {
            if (string.IsNullOrEmpty(text) ||
                item.Name.Contains(text, StringComparison.OrdinalIgnoreCase))
            {
                DisplayItems.Add(item);
            }
        }
    }
}

public class ProviderItem
{
    public StorageProviderType ProviderType { get; set; }
    public string DisplayName { get; set; } = string.Empty;
}
