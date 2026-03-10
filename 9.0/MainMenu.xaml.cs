namespace stars_beyond;

public partial class MainMenu : ContentPage
{
    MusicService musicService;
    public MainMenu(MusicService service)
    {
        InitializeComponent();
        musicService = service;
    }
    protected override async void OnAppearing()
    {
        base.OnAppearing();

        await musicService.PlayMusic();
    }
    private async void GoToGameplay(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//GameplayPageMainUI");
        await musicService.FadeOut(2000);
    }
    private async void GoToOptions(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//OptionsPage");
    }
    private async void GoToCredits(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//CreditsPage");
    }
    private void Exit(object sender, EventArgs e)
    {
        Application.Current.Quit();
    } 
}