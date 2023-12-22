namespace SukaliCareApp.Views;

public partial class ReadingsView : ContentPage
{
    private readonly CGMReadingsViewModel viewModel;

    public ReadingsView(CGMReadingsViewModel viewModel)
	{
		InitializeComponent();
		BindingContext = viewModel;
        this.viewModel = viewModel;
    }
    protected override void OnAppearing()
    {
        base.OnAppearing();
        viewModel.OnRefreshing();

    }
}