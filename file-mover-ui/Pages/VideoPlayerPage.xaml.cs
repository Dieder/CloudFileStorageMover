namespace FileMover.UI.Pages;

public partial class VideoPlayerPage : ContentPage
{
	public VideoPlayerPage()
	{
		InitializeComponent();
	}

	protected override void OnAppearing()
	{
		base.OnAppearing();
		VideoTitle.Text = "Video File";
		VideoInfo.Text = $"Format: {GetVideoFormat()}\nThe video will be transferred with your selected files.";
	}

	private static string GetVideoFormat()
	{
		return "MP4 (supported formats: mp4, mov, avi, mkv, wmv, m4v, flv, rm, rmvb)";
	}

	private async void OnCloseClicked(object sender, EventArgs e)
	{
		await Shell.Current.GoToAsync("..");
	}
}
