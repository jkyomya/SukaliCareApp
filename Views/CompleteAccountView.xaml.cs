namespace SukaliCareApp.Views;

public partial class CompleteAccountView : ContentPage
{
    private readonly CompleteAccountViewModel viewModel;

    public CompleteAccountView(CompleteAccountViewModel viewModel)
	{
		InitializeComponent();
		BindingContext = viewModel;
        this.viewModel = viewModel;
    }
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await viewModel.RemoteCheckUser();
    }
}