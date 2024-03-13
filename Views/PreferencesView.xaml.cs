namespace SukaliCareApp.Views;

public partial class PreferencesView : ContentPage
{
    private readonly PreferencesViewModel viewModel;

    public PreferencesView(PreferencesViewModel viewModel)
	{
		InitializeComponent();
		BindingContext = viewModel;
        this.viewModel = viewModel;
    }
    protected override void OnAppearing()
    {
        base.OnAppearing();
        viewModel.OnAppearing();
    }

    private void Notification_Switch_Toggled(object sender, ToggledEventArgs e)
    {
        viewModel.NotificationToggled(e.Value);
    }

    private void Connect_Switch_Toggled(object sender, ToggledEventArgs e)
    {
        viewModel.ConnectionToggled(e.Value);
    }
}