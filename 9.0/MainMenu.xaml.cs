namespace stars_beyond;

public partial class MainMenu : ContentPage
{
    MusicService musicService;
    SfxService sfxService;
    public MainMenu(MusicService service, SfxService sfx)
    {
        InitializeComponent();
        musicService = service;
        sfxService = sfx;
    }
    protected override async void OnAppearing()
    {
        base.OnAppearing();

        await musicService.PlayMusic();
        await sfxService.Initialize();
    }
    private async void GoToGameplay(object sender, EventArgs e)
{
    sfxService.PlayStart();

    _ = musicService.FadeOut(2000);

    await FadeOverlay.FadeTo(1, 1000, Easing.CubicIn);
    await Shell.Current.GoToAsync("//GameplayPageMainUI");
}

    private async void GoToOptions(object sender, EventArgs e)
    {
        sfxService.PlayMenuClick();
        await Shell.Current.GoToAsync("//OptionsPage");
    }

    private async void GoToCredits(object sender, EventArgs e)
    {
        sfxService.PlayMenuClick();
        await Shell.Current.GoToAsync("//CreditsPage");
    }

    private async void Exit(object sender, EventArgs e)
    {
        sfxService.PlayMenuClick();
        Application.Current.Quit();
    }
}