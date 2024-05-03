using Shiny;
using Shiny.BluetoothLE;

namespace SukaliCareApp.Services;

public class BluetoothDelegate : BleDelegate
{
    public override Task OnAdapterStateChanged(AccessState state)
    {
        return base.OnAdapterStateChanged(state);
    }

    public override Task OnPeripheralStateChanged(IPeripheral peripheral)
    {
        return base.OnPeripheralStateChanged(peripheral);
    }
}
