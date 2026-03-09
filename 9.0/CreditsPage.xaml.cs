namespace stars_beyond;

public partial class CreditsPage : ContentPage
{
	public CreditsPage()
	{
		InitializeComponent();
	}
    private async void GoToMain(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//MainMenu");
    }
}