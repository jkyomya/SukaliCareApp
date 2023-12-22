namespace SukaliCareApp.Views;

public partial class DevicesView : ContentPage
{
    private readonly DeviceMgtViewModel viewModel;

    public DevicesView(DeviceMgtViewModel viewModel)
	{
		InitializeComponent();
        this.viewModel = viewModel;
        BindingContext = viewModel;
    }
}