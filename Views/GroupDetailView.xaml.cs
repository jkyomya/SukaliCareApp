namespace SukaliCareApp.Views;

public partial class GroupDetailView : ContentPage
{
    public GroupDetailView(GroupDetailViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
