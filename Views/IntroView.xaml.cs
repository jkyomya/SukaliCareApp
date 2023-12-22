namespace SukaliCareApp.Views;

public partial class IntroView : ContentPage
{
    private readonly IntroViewModel viewModel;

    public IntroView(IntroViewModel viewModel)
	{
		InitializeComponent();
        this.viewModel = viewModel;
        BindingContext = viewModel;
    }
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await viewModel.OnAppearing();
    }
}