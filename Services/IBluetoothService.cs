using Plugin.BLE.Abstractions.Contracts;

namespace SukaliCareApp.Services;

public interface IBluetoothService
{
    IDevice[] GetConnectedDevices();
    Task Connect(string deviceName);
    Task Disconnect();
    Task Send(string deviceName, byte[] message);
    Task<byte[]> Receive(string deviceName);
}
