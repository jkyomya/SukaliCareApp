namespace SukaliCareApp.Views;

public partial class DashboardView : ContentPage
{
    private readonly DashboardViewModel viewModel;

    public DashboardView(DashboardViewModel viewModel)
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
}