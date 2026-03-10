namespace stars_beyond;

public partial class OptionsPage : ContentPage
{
	public OptionsPage(MusicService service)
	{
        InitializeComponent();
        musicService = service;
        this.Loaded += (s, e) =>
        {
            SetSliderFromPreferences();
        };
    }
    MusicService musicService;

    private async void GoToMain(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//MainMenu");
    }
    void SelectPolish(object sender, EventArgs e)
    {
        radioPolish.Source = "radio_checked.png";
        radioEnglish.Source = "radio_unchecked.png";
    }
    void SelectEnglish(object sender, EventArgs e)
    {
        radioPolish.Source = "radio_unchecked.png";
        radioEnglish.Source = "radio_checked.png";
    }
    void SelectArrows(object sender, EventArgs e)
    {
        radioJoystick.Source = "radio_unchecked.png";
        radioArrows.Source = "radio_checked.png";
    }
    void SelectJoystick(object sender, EventArgs e)
    {
        radioArrows.Source = "radio_unchecked.png";
        radioJoystick.Source = "radio_checked.png";
    }
    double musicValue = 0.5;

    double startX;
    int steps = 10;

    void OnPanUpdated(object sender, PanUpdatedEventArgs e)
    {
        var knob = sender as Image;

        switch (e.StatusType)
        {
            case GestureStatus.Started:
                startX = knob.TranslationX;
                break;
            case GestureStatus.Running:
                double newX = startX + e.TotalX;
                double maxX = MusicSlider.Width - knob.Width - 20;
                newX = Math.Clamp(newX, 0, maxX);
                double stepSize = maxX / steps;
                newX = Math.Round(newX / stepSize) * stepSize;
                knob.TranslationX = newX;
                double value = newX / maxX;
                if (knob == MusicKnob)
                {
                    musicService.SetVolume(value);
                    Preferences.Set("musicVolume", value);
                }

                if (knob == SFXKnob)
                {
                    
                }
                break;
        }
    }
    void SetSliderFromPreferences()
    {
        double savedVolume = Preferences.Get("musicVolume", 0.5);

        double maxX = MusicSlider.Width - MusicKnob.Width - 20;

        double newX = savedVolume * maxX;

        double stepSize = maxX / steps;
        newX = Math.Round(newX / stepSize) * stepSize;

        MusicKnob.TranslationX = newX;
    }
}