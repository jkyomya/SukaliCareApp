using Mopups.Pages;

namespace SukaliCareApp.Views;

public partial class AddDeviceView : PopupPage
{
    private readonly AddDeviceViewModel viewModel;

    public AddDeviceView(AddDeviceViewModel viewModel)
	{
		InitializeComponent();
        this.viewModel = viewModel;
        BindingContext = viewModel;
    }
}