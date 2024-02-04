using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Mvvm.Messaging;
using Mopups.Services;
using Shiny;
using Shiny.BluetoothLE;
using Shiny.BluetoothLE.Managed;
using System.Reactive.Linq;
using Permissions = Microsoft.Maui.ApplicationModel.Permissions;

namespace SukaliCareApp.ViewModels;

public partial class AddDeviceViewModel : BaseViewModel
{
    private readonly IBleManager bleManager;
    private readonly IManagedScan scanner;

    public ObservableCollection<IPeripheral> Peripherals { get; } = [];
    public ObservableCollection<CGMDevice> ScannedDevices { get; } = [];

    [ObservableProperty]
    private bool isScanning = false;

    public AddDeviceViewModel(IBleManager bleManager)
    {
        this.bleManager = bleManager;
        scanner = bleManager.CreateManagedScanner();
    }

    

    [RelayCommand]
    public async Task CloseDialog()
    {
        scanner.Stop();
        scanner.Dispose();

        await MopupService.Instance.PopAllAsync();
    }

    [RelayCommand]
    public async Task ScanDevices()
    {
        var access = await bleManager.RequestAccessAsync();
        if (access != AccessState.Available)
        {
            await Permissions.RequestAsync<Permissions.Bluetooth>();
        }
        //try
        //{
        //    IsScanning = true;
        //    var scanSession = bleManager.Scan()
        //        .Subscribe(result =>
        //        {
        //            if(!string.IsNullOrEmpty(result.Peripheral.Name))
        //            {
        //                MainThread.BeginInvokeOnMainThread(() =>
        //                {
        //                    Peripherals.Add(result.Peripheral);
        //                    ScannedDevices.Add(new CGMDevice
        //                    {
        //                        DeviceId = result.Peripheral.Uuid.ToString() ?? Guid.NewGuid().ToString(),
        //                        DeviceName = result.Peripheral.Name,
        //                        Uuid = result.Peripheral.Uuid.ToString() ?? string.Empty
        //                    });
        //                });

        //            }
        //        });
        //    scanSession.Dispose();
        //}
        //catch (Exception ex)
        //{
        //    await Shell.Current.DisplayAlert(
        //          "Scan Error",
        //          ex.Message,
        //          "Ok");
        //}

        try
        {
            await scanner.Start(
                new ScanConfig(),
                result =>
                {
                    if (!string.IsNullOrEmpty(result.Peripheral.Name))
                    {
                        var peripheral = result.Peripheral;
                        Peripherals.Add(peripheral);
                        MainThread.BeginInvokeOnMainThread(async () =>
                        {
                            await Shell.Current.DisplaySnackbar($"Found {peripheral.Name}");
                            ScannedDevices.Add(new CGMDevice
                            {
                                DeviceId = peripheral.Uuid.ToString() ?? Guid.NewGuid().ToString(),
                                DeviceName = peripheral.Name,
                                Uuid = peripheral.Uuid.ToString() ?? string.Empty
                            });
                        });

                        return true;
                    }

                    return false;
                },
                RxApp.MainThreadScheduler,
                null,
                TimeSpan.FromSeconds(10)
                );
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert(
                  "Scan Error",
                  ex.Message,
                  "Ok");
        }

        IsScanning = scanner.IsScanning;


    }

    [RelayCommand]
    public void StopScan()
    {
        scanner.Stop();
        IsScanning = scanner.IsScanning;
        scanner.Dispose();
    }

    [RelayCommand]
    public async Task ConnectDevice(IPeripheral peripheral)
    {
        bool result = await Shell.Current.DisplayAlert(
            "Connect Device",
            $"Do you want to connect to {peripheral.Name}?", "Yes", "No");
        if (result)
        {
            try
            {
                await peripheral.WithConnectIf(new ConnectionConfig(true))
                    .Timeout(TimeSpan.FromSeconds(10));
            }
            catch(TimeoutException)
            {
                await Shell.Current.DisplayAlert(
                      "Connection Error",
                      $"Could not connect to {peripheral.Name}. Please try again.", "Ok");
            }
        }

    }

    //[RelayCommand]
    //public async Task PairWithDevice(IPeripheral peripheral)
    //{
    //    var status = peripheral.TryGetPairingStatus();

    //    if (status == PairingState.NotPaired)
    //    {
    //        var result = await peripheral.TryPairingRequest();
    //        if (result is not null && (bool)result)
    //        {
    //            await ConnectDevice(peripheral);

    //            WeakReferenceMessenger.Default.Send(new CGMDevice
    //            {
    //                Uuid = peripheral.Uuid.ToString(),
    //                DeviceId = peripheral.Uuid.ToString(),
    //                DeviceName = peripheral.Name
    //            });
    //        }
    //    }
    //    else
    //    {
    //        await Shell.Current.DisplayAlert(
    //              "Pairing Error",
    //              $"Device {peripheral.Name} is already paired.", "Ok");
    //        return;
    //    }
    //}
}
