using Microsoft.Maui.Storage;

namespace stars_beyond;

public partial class IntroPage : ContentPage
{
    public IntroPage()
    {
        InitializeComponent();
    }

    private async void PageLoaded(object sender, EventArgs e)
    {
        await Task.Delay(500); // opóźnienie 0.5 sekundy

        var file = await FileSystem.OpenAppPackageFileAsync("intro.mp4");
        var tempPath = Path.Combine(FileSystem.CacheDirectory, "intro.mp4");

        using (var stream = File.Create(tempPath))
        {
            await file.CopyToAsync(stream);
        }

        IntroVideo.Source = tempPath;
        IntroVideo.Play();
    }

    private void VideoEnded(object sender, EventArgs e)
    {
        MainThread.BeginInvokeOnMainThread(async () =>
        {
            await Shell.Current.GoToAsync("//MainMenu");
        });
    }
}