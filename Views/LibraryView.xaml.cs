namespace SukaliCareApp.Views;

public partial class LibraryView : ContentPage
{
    private readonly LibraryViewModel viewModel;

    public LibraryView(LibraryViewModel viewModel)
	{
		InitializeComponent();
        this.viewModel = viewModel;
        BindingContext = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        viewModel.OnAppearing();
    }
}