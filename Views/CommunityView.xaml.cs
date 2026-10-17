namespace SukaliCareApp.Views;

public partial class CommunityView : ContentPage
{
    private readonly CommunityViewModel viewModel;

    public CommunityView(CommunityViewModel viewModel)
    {
        InitializeComponent();
        this.viewModel = viewModel;
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (viewModel.Groups.Count == 0)
        {
            await viewModel.RefreshAsync();
        }
    }
}
