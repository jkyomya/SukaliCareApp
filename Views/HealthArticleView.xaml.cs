namespace SukaliCareApp.Views;

public partial class HealthArticleView : ContentPage
{
    public HealthArticleView(HealthArticleViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
