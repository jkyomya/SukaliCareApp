namespace SukaliCareApp.Views;

public partial class HealthInfoView : ContentPage
{
    private readonly HealthInfoViewModel viewModel;

    public HealthInfoView(HealthInfoViewModel viewModel)
    {
        InitializeComponent();
        this.viewModel = viewModel;
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await viewModel.LoadAsync();
    }
}
