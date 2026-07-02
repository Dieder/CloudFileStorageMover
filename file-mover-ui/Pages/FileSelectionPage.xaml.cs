using CommunityToolkit.Maui.Storage;
using FileMover.UI.Models;
using FileMover.UI.Services;
using FileMover.UI.ViewModels;

namespace FileMover.UI.Pages;

public partial class FileSelectionPage : ContentPage
{
	private readonly FileSelectionViewModel viewModel;
	private readonly TransferSessionService sessionService;
	private string? currentLocationId;
	private string currentFilter = "all";

	private static readonly HashSet<string> ImageExtensions = new(StringComparer.OrdinalIgnoreCase) { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".webp" };
	private static readonly HashSet<string> DocExtensions = new(StringComparer.OrdinalIgnoreCase) { ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".ppt", ".pptx", ".txt", ".csv" };
	private static readonly HashSet<string> VideoExtensions = new(StringComparer.OrdinalIgnoreCase) { ".mp4", ".mov", ".avi", ".mkv", ".wmv", ".m4v", ".flv", ".rm", ".rmvb" };

	private Button[] filterButtons = Array.Empty<Button>();
	private Color primaryColor = Color.FromArgb("#512BD4");

	public FileSelectionPage(FileSelectionViewModel viewModel, TransferSessionService sessionService)
	{
		InitializeComponent();
		this.viewModel = viewModel;
		this.sessionService = sessionService;
		BindingContext = viewModel;

		filterButtons = new[] { FilterAllBtn, FilterImagesBtn, FilterDocsBtn, FilterVideosBtn };
	}

	protected override async void OnAppearing()
	{
		base.OnAppearing();
		if (sessionService.SourceLocation != null)
		{
			currentLocationId = sessionService.SourceLocation.Id;
			await viewModel.LoadFilesAsync(currentLocationId, sessionService.SourceLocation.Path);
		}
		viewModel.PropertyChanged += OnViewModelPropertyChanged;
	}

	protected override void OnDisappearing()
	{
		base.OnDisappearing();
		viewModel.PropertyChanged -= OnViewModelPropertyChanged;
	}

	private async void OnViewModelPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
	{
		if (e.PropertyName == nameof(FileSelectionViewModel.SelectedFile))
		{
			await UpdatePreviewPaneAsync(viewModel.SelectedFile);
		}
	}

	private async Task UpdatePreviewPaneAsync(FileItem? file)
	{
		PreviewImage.IsVisible = false;
		PreviewPlaceholder.IsVisible = true;
		ExifMetadataPanel.IsVisible = false;
		EditImageBtn.IsVisible = false;

		if (file == null) return;

		PreviewFileName.Text = file.Name;
		PreviewFileSize.Text = file.SizeDisplay;
		PreviewFileDate.Text = $"Modified: {file.Modified:g}";

		var ext = Path.GetExtension(file.Name).ToLowerInvariant();

		if (ImageExtensions.Contains(ext))
		{
			PreviewPlaceholder.IsVisible = false;
			PreviewImage.IsVisible = true;
			EditImageBtn.IsVisible = true;
			PreviewImage.Source = ImageSource.FromFile(file.Path);

			await LoadExifMetadataAsync(file);
		}
		else if (VideoExtensions.Contains(ext))
		{
			PreviewPlaceholder.Text = "🎬 Video file - tap to preview";
			var tapGesture = new TapGestureRecognizer();
			tapGesture.Tapped += async (s, e) =>
			{
				await Shell.Current.GoToAsync($"video-player?videoPath={Uri.EscapeDataString(file.Path)}");
			};
			PreviewPlaceholder.GestureRecognizers.Clear();
			PreviewPlaceholder.GestureRecognizers.Add(tapGesture);
		}
		else
		{
			PreviewPlaceholder.Text = DocExtensions.Contains(ext) ? "📄 Document file" : "No preview available";
		}
	}

	private async Task LoadExifMetadataAsync(FileItem file)
	{
		try
		{
			if (!File.Exists(file.Path)) return;

			var fileInfo = new FileInfo(file.Path);
			ExifMetadataPanel.IsVisible = true;
			ExifCamera.Text = "Camera: (from EXIF)";
			ExifDate.Text = $"Date: {fileInfo.LastWriteTime:yyyy-MM-dd HH:mm}";
			ExifDimensions.Text = $"Size: {fileInfo.Length} bytes";
			ExifAperture.Text = "";
			ExifExposure.Text = "";
			ExifIso.Text = "";
		}
		catch
		{
			ExifMetadataPanel.IsVisible = false;
		}
	}

	private void ApplyFilter(string filter)
	{
		currentFilter = filter;

		var filtered = filter switch
		{
			"images" => viewModel.AllFiles.Where(f => !f.IsFolder && ImageExtensions.Contains(Path.GetExtension(f.Name))).ToList(),
			"documents" => viewModel.AllFiles.Where(f => !f.IsFolder && DocExtensions.Contains(Path.GetExtension(f.Name))).ToList(),
			"videos" => viewModel.AllFiles.Where(f => !f.IsFolder && VideoExtensions.Contains(Path.GetExtension(f.Name))).ToList(),
			_ => viewModel.AllFiles
		};

		viewModel.Files.Clear();
		foreach (var f in filtered)
			viewModel.Files.Add(f);

		UpdateFilterButtonStyles(filter);
	}

	private void UpdateFilterButtonStyles(string activeFilter)
	{
		var filterMap = new Dictionary<string, Button>
		{
			["all"] = FilterAllBtn,
			["images"] = FilterImagesBtn,
			["documents"] = FilterDocsBtn,
			["videos"] = FilterVideosBtn
		};

		foreach (var kvp in filterMap)
		{
			if (kvp.Key == activeFilter)
			{
				kvp.Value.BackgroundColor = primaryColor;
				kvp.Value.TextColor = Colors.White;
				kvp.Value.BorderWidth = 0;
			}
			else
			{
				kvp.Value.BackgroundColor = Colors.Transparent;
				kvp.Value.TextColor = primaryColor;
				kvp.Value.BorderColor = primaryColor;
				kvp.Value.BorderWidth = 1;
			}
		}
	}

	private void OnFilterAllClicked(object sender, EventArgs e)
	{
		ApplyFilter("all");
	}

	private void OnFilterImagesClicked(object sender, EventArgs e) => ApplyFilter("images");
	private void OnFilterDocsClicked(object sender, EventArgs e) => ApplyFilter("documents");
	private void OnFilterVideosClicked(object sender, EventArgs e) => ApplyFilter("videos");

	private void OnSelectAllClicked(object sender, EventArgs e)
	{
		viewModel.SelectAll();
	}

	private void OnDeselectAllClicked(object sender, EventArgs e)
	{
		viewModel.DeselectAll();
	}

	private async void OnPickFolderClicked(object sender, EventArgs e)
	{
		try
		{
			var result = await FolderPicker.Default.PickAsync(CancellationToken.None);
			if (result.IsSuccessful && result.Folder != null)
			{
				await viewModel.NavigateToFolderAsync(new FileItem
				{
					Name = result.Folder.Name,
					Path = result.Folder.Path,
					IsFolder = true
				});
				ApplyFilter("all");
			}
		}
		catch (Exception ex)
		{
			await DisplayAlert("Error", $"Failed to pick folder: {ex.Message}", "OK");
		}
	}

	private async void OnCameraClicked(object sender, EventArgs e)
	{
		try
		{
			if (!MediaPicker.Default.IsCaptureSupported)
			{
				await DisplayAlert("Not Supported", "Camera capture is not supported on this device", "OK");
				return;
			}

			var photo = await MediaPicker.Default.CapturePhotoAsync();
			if (photo != null)
			{
				var localPath = Path.Combine(FileSystem.AppDataDirectory, photo.FileName);
				using var sourceStream = await photo.OpenReadAsync();
				using var localStream = File.OpenWrite(localPath);
				await sourceStream.CopyToAsync(localStream);

				await DisplayAlert("Photo Captured", $"Saved: {photo.FileName}", "OK");

				if (currentLocationId != null)
					await viewModel.LoadFilesAsync(currentLocationId);
			}
		}
		catch (PermissionException)
		{
			await DisplayAlert("Permission Denied", "Camera permission is required to capture photos", "OK");
		}
		catch (Exception ex)
		{
			await DisplayAlert("Error", $"Failed to capture photo: {ex.Message}", "OK");
		}
	}

	private async void OnEditImageClicked(object sender, EventArgs e)
	{
		if (viewModel.SelectedFile == null) return;
		await Shell.Current.GoToAsync("image-editor");
	}

	private void OnContinueClicked(object sender, EventArgs e)
	{
		if (viewModel.SelectedFiles.Count == 0)
		{
			DisplayAlert("No Files Selected", "Please select at least one file", "OK");
			return;
		}

		sessionService.SelectedFiles = viewModel.SelectedFiles.ToList();
		Shell.Current.GoToAsync("transfer-mode");
	}

	private async void OnBackClicked(object sender, EventArgs e)
	{
		await Shell.Current.GoToAsync("//mode-selection/source-selection");
	}

	private async void OnFileSelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		if (e.CurrentSelection.FirstOrDefault() is FileItem selected && selected.IsFolder)
		{
			if (sender is CollectionView cv)
				cv.SelectedItem = null;

			await viewModel.NavigateToFolderAsync(selected);
			ApplyFilter("all");
		}
	}

	private async void OnSettingsClicked(object sender, EventArgs e)
	{
		await Shell.Current.GoToAsync("credentials-config");
	}

	private async void OnBackToParentClicked(object sender, EventArgs e)
	{
		await viewModel.NavigateBackAsync();
		ApplyFilter("all");
	}
}
