namespace SukaliCareApp.Views;

public partial class SignupView : ContentPage
{
	public SignupView(SignupViewModel viewModel)
	{
		InitializeComponent();
		BindingContext = viewModel;
	}
}