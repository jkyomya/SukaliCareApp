

using Mopups.Interfaces;
using Mopups.Services;

namespace SukaliCareApp.ViewModels
{
    public partial class DeviceMgtViewModel : BaseViewModel
    {
        private readonly CgmDataService dataService;
        private readonly IPopupNavigation popupNavigation;
        [ObservableProperty]
        ObservableCollection<CGMDevice> pairedDevices;

        public DeviceMgtViewModel(CgmDataService dataService, IPopupNavigation popupNavigation)
        {
            this.dataService = dataService;
            this.popupNavigation = popupNavigation;
        }
        public async Task LoadDataAsync()
        {
            PairedDevices = new ObservableCollection<CGMDevice>(await dataService.GetPairedDevices());
        }

        [RelayCommand]
        public async Task ScanDevicesAsync()
        {
            await popupNavigation.PushAsync(new AddDeviceView(new AddDeviceViewModel()));
        }
    }
}
