namespace stars_beyond;

public partial class MainMenu : ContentPage
{
    public MainMenu()
    {
        InitializeComponent();
    }
    private async void GoToGameplay(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//GameplayPage");
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