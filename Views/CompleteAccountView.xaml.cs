namespace SukaliCareApp.Views;

public partial class CompleteAccountView : ContentPage
{
	public CompleteAccountView(CompleteAccountViewModel viewModel)
	{
		InitializeComponent();
		BindingContext = viewModel;
	}
}