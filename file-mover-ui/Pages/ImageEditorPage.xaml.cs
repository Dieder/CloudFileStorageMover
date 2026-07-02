namespace FileMover.UI.Pages;

public partial class ImageEditorPage : ContentPage
{
	private string? currentFilePath;
	private int rotationAngle;
	private int versionCount;

	public ImageEditorPage()
	{
		InitializeComponent();
	}

	protected override void OnAppearing()
	{
		base.OnAppearing();
		LoadCurrentImage();
	}

	private void LoadCurrentImage()
	{
		NoImageLabel.IsVisible = true;
		EditorImage.IsVisible = false;
	}

	private void OnRotateRightClicked(object sender, EventArgs e)
	{
		rotationAngle = (rotationAngle + 90) % 360;
		EditorImage.Rotation = rotationAngle;
	}

	private void OnRotateLeftClicked(object sender, EventArgs e)
	{
		rotationAngle = (rotationAngle - 90 + 360) % 360;
		EditorImage.Rotation = rotationAngle;
	}

	private void OnFlipClicked(object sender, EventArgs e)
	{
		EditorImage.ScaleX = EditorImage.ScaleX == -1 ? 1 : -1;
	}

	private void OnCropOriginalClicked(object sender, EventArgs e)
	{
		EditorImage.Aspect = Aspect.AspectFit;
	}

	private void OnCropSquareClicked(object sender, EventArgs e)
	{
		EditorImage.Aspect = Aspect.AspectFill;
	}

	private void OnCrop4x3Clicked(object sender, EventArgs e)
	{
		EditorImage.Aspect = Aspect.AspectFill;
	}

	private void OnCrop16x9Clicked(object sender, EventArgs e)
	{
		EditorImage.Aspect = Aspect.AspectFill;
	}

	private async void OnSaveClicked(object sender, EventArgs e)
	{
		versionCount++;

		var originalName = Path.GetFileNameWithoutExtension(currentFilePath ?? "image");
		var ext = Path.GetExtension(currentFilePath ?? ".jpg");
		var versionedName = $"{originalName}_v{versionCount}{ext}";

		VersionLabel.Text = $"Saved as: {versionedName}";
		VersionLabel.IsVisible = true;

		await DisplayAlert("Saved", $"Image saved as {versionedName}", "OK");

		await GoBackToSourceSelectionAsync();
	}

	private async void OnCancelClicked(object sender, EventArgs e)
	{
		await GoBackToSourceSelectionAsync();
	}

	private async Task GoBackToSourceSelectionAsync()
	{
		var segments = Shell.Current.CurrentState.Location.ToString()
			.Split('/', StringSplitOptions.RemoveEmptyEntries);
		var canPopTwo = segments.Length >= 4;
		await Shell.Current.GoToAsync(canPopTwo ? "../.." : "..");
	}

	protected override bool OnBackButtonPressed()
	{
		MainThread.BeginInvokeOnMainThread(async () =>
		{
			await GoBackToSourceSelectionAsync();
		});
		return true;
	}
}
