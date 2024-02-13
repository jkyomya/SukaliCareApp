using Mopups.Pages;

namespace SukaliCareApp.Views;

public partial class AddDeviceView : PopupPage
{
    private readonly AddDeviceViewModel _viewModel;

    public AddDeviceView(AddDeviceViewModel viewModel)
	{
		InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
       await _viewModel.ScanDevices();
    }
}