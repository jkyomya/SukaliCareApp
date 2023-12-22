using Shiny;
using Shiny.BluetoothLE;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SukaliCareApp.Services
{
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
}
